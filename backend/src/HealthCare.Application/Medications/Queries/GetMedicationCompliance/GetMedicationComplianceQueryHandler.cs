using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Queries.GetMedicationCompliance;

public class GetMedicationComplianceQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMedicationComplianceQuery, List<ComplianceReportDto>>
{
    public async Task<List<ComplianceReportDto>> Handle(GetMedicationComplianceQuery request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var since = DateTime.UtcNow.AddDays(-7 * request.WeeksBack);

        var logs = await db.MedicationLogs
            .Include(l => l.MedicationSchedule)
                .ThenInclude(s => s.Medication)
            .Where(l =>
                l.MedicationSchedule.Medication.HealthProfileId == profile.Id &&
                l.MedicationSchedule.Medication.DeletedAt == null &&
                l.ScheduledAt >= since &&
                l.Status != MedicationLogStatus.Pending)
            .ToListAsync(cancellationToken);

        var result = new List<ComplianceReportDto>();
        for (int i = request.WeeksBack - 1; i >= 0; i--)
        {
            var weekStart = DateTime.UtcNow.Date.AddDays(-7 * (i + 1) + 1);
            var weekEnd = weekStart.AddDays(6);
            var weekLogs = logs.Where(l => l.ScheduledAt.Date >= weekStart && l.ScheduledAt.Date <= weekEnd).ToList();
            var total = weekLogs.Count;
            var taken = weekLogs.Count(l => l.Status == MedicationLogStatus.Taken);
            result.Add(new ComplianceReportDto
            {
                WeekLabel = $"{weekStart:dd/MM} – {weekEnd:dd/MM}",
                TotalScheduled = total,
                TotalTaken = taken,
                CompliancePercent = total > 0 ? Math.Round((decimal)taken / total * 100, 1) : 0
            });
        }
        return result;
    }
}
