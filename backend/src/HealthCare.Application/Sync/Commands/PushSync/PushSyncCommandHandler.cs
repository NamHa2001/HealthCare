using System.Text.Json;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Sync.Commands.PushSync;

public class PushSyncCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    ILogger<PushSyncCommandHandler> logger)
    : IRequestHandler<PushSyncCommand, PushSyncResultDto>
{
    public async Task<PushSyncResultDto> Handle(PushSyncCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        int applied = 0, skipped = 0;
        var errors = new List<string>();

        foreach (var op in request.Operations)
        {
            try
            {
                switch (op.EntityType)
                {
                    case "health_measurement":
                        await ApplyMeasurementAsync(op, profile, db, ct);
                        applied++;
                        break;

                    case "blood_pressure":
                        await ApplyBloodPressureAsync(op, profile, db, ct);
                        applied++;
                        break;

                    default:
                        skipped++;
                        logger.LogWarning("Unknown entity type in sync: {Type}", op.EntityType);
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{op.EntityType}/{op.EntityId}: {ex.Message}");
                logger.LogError(ex, "Sync operation failed for {EntityType}/{EntityId}", op.EntityType, op.EntityId);
            }
        }

        if (applied > 0) await db.SaveChangesAsync(ct);

        return new PushSyncResultDto(
            applied, skipped, errors,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private static Task ApplyMeasurementAsync(
        Sync.DTOs.SyncOperationDto op,
        HealthProfile? profile,
        IApplicationDbContext db,
        CancellationToken ct)
    {
        if (op.Operation != "create" || profile is null || op.Payload is null)
            return Task.CompletedTask;

        var json = JsonSerializer.Serialize(op.Payload);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var measuredAt = root.TryGetProperty("measuredAt", out var ts) && ts.TryGetDateTime(out var dt)
            ? dt : DateTime.UtcNow;

        var m = HealthMeasurement.Create(
            profile.Id,
            measuredAt,
            weightKg:         root.TryGetProperty("weightKg",        out var w)  && w.TryGetDecimal(out var wv)  ? wv  : null,
            heightCm:         root.TryGetProperty("heightCm",        out var h)  && h.TryGetDecimal(out var hv)  ? hv  : null,
            heartRateBpm:     root.TryGetProperty("heartRateBpm",    out var hr) && hr.TryGetInt32(out var hrv)  ? hrv : null,
            bodyTemperature:  root.TryGetProperty("bodyTemperature", out var bt) && bt.TryGetDecimal(out var btv) ? btv : null,
            bloodGlucose:     root.TryGetProperty("bloodGlucose",    out var bg) && bg.TryGetDecimal(out var bgv) ? bgv : null,
            spo2Percent:      root.TryGetProperty("spo2Percent",     out var sp) && sp.TryGetDecimal(out var spv) ? spv : null,
            source:           "sync");

        db.HealthMeasurements.Add(m);
        return Task.CompletedTask;
    }

    private static Task ApplyBloodPressureAsync(
        Sync.DTOs.SyncOperationDto op,
        HealthProfile? profile,
        IApplicationDbContext db,
        CancellationToken ct)
    {
        if (op.Operation != "create" || profile is null || op.Payload is null)
            return Task.CompletedTask;

        var json = JsonSerializer.Serialize(op.Payload);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("systolic",  out var sys) || !sys.TryGetInt32(out var sysV)) return Task.CompletedTask;
        if (!root.TryGetProperty("diastolic", out var dia) || !dia.TryGetInt32(out var diaV)) return Task.CompletedTask;

        var measuredAt = root.TryGetProperty("measuredAt", out var ts) && ts.TryGetDateTime(out var dt)
            ? dt : DateTime.UtcNow;

        int? pulse = root.TryGetProperty("pulse", out var p) && p.TryGetInt32(out var pv) ? pv : null;

        var bp = BloodPressureLog.Create(profile.Id, measuredAt, sysV, diaV, pulse);
        db.BloodPressureLogs.Add(bp);
        return Task.CompletedTask;
    }
}
