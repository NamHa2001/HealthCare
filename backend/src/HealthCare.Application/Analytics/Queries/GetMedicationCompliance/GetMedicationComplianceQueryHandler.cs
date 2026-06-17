using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HealthCare.Application.Analytics.Queries.GetMedicationCompliance;

public class GetMedicationComplianceQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMedicationComplianceQuery, IReadOnlyList<ComplianceWeekDto>>
{
    public async Task<IReadOnlyList<ComplianceWeekDto>> Handle(
        GetMedicationComplianceQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var weeks = Math.Clamp(request.Weeks, 1, 24);
        var from = DateTime.UtcNow.AddDays(-weeks * 7);

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return [];

        var logs = await db.MedicationLogs
            .AsNoTracking()
            .Where(l => l.MedicationSchedule.Medication.HealthProfileId == profile.Id
                && l.ScheduledAt >= from
                && (l.Status == MedicationLogStatus.Taken || l.Status == MedicationLogStatus.Skipped))
            .Select(l => new { l.ScheduledAt, l.Status })
            .ToListAsync(ct);

        // Nhóm theo tuần
        var grouped = logs
            .GroupBy(l => GetWeekStart(l.ScheduledAt))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var total = g.Count();
                var taken = g.Count(x => x.Status == MedicationLogStatus.Taken);
                var rate = total > 0 ? Math.Round((double)taken / total * 100, 1) : 0;
                return new ComplianceWeekDto(g.Key.ToString("dd/MM"), rate, total, taken);
            })
            .ToList();

        return grouped;
    }

    private static DateTime GetWeekStart(DateTime d)
    {
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff).Date;
    }
}
