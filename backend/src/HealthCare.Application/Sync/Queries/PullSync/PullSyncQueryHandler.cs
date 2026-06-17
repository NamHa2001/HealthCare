using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sync.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sync.Queries.PullSync;

public class PullSyncQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<PullSyncQuery, PullSyncResponseDto>
{
    public async Task<PullSyncResponseDto> Handle(PullSyncQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var sinceUtc = DateTimeOffset.FromUnixTimeMilliseconds(request.Since).UtcDateTime;

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var changes = new List<SyncChangeDto>();

        if (profile is not null)
        {
            // Health measurements
            var measurements = await db.HealthMeasurements
                .AsNoTracking()
                .Where(m => m.HealthProfileId == profile.Id && m.CreatedAt > sinceUtc)
                .Select(m => new { m.Id, m.MeasuredAt, m.WeightKg, m.HeightCm, m.Bmi,
                                   m.BloodGlucose, m.HeartRateBpm, m.Spo2Percent,
                                   m.BodyTemperature, m.CreatedAt })
                .ToListAsync(ct);

            changes.AddRange(measurements.Select(m => new SyncChangeDto(
                "health_measurement", m.Id.ToString(), "create", m, m.CreatedAt)));

            // Blood pressure
            var bpLogs = await db.BloodPressureLogs
                .AsNoTracking()
                .Where(b => b.HealthProfileId == profile.Id && b.CreatedAt > sinceUtc)
                .Select(b => new { b.Id, b.MeasuredAt, b.Systolic, b.Diastolic, b.Pulse, b.CreatedAt })
                .ToListAsync(ct);

            changes.AddRange(bpLogs.Select(b => new SyncChangeDto(
                "blood_pressure", b.Id.ToString(), "create", b, b.CreatedAt)));

            // Vaccine records
            var vaccines = await db.VaccineRecords
                .AsNoTracking()
                .Where(v => v.HealthProfileId == profile.Id && v.CreatedAt > sinceUtc)
                .Select(v => new { v.Id, v.VaccineName, v.DoseNumber, v.InjectionDate,
                                   v.NextDueDate, v.CreatedAt })
                .ToListAsync(ct);

            changes.AddRange(vaccines.Select(v => new SyncChangeDto(
                "vaccine_record", v.Id.ToString(), "create", v, v.CreatedAt)));
        }

        return new PullSyncResponseDto(
            changes.OrderBy(c => c.ServerTimestamp).ToList(),
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}
