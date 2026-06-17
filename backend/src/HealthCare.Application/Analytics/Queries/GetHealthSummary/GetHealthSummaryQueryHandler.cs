using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetHealthSummary;

public class GetHealthSummaryQueryHandler : IRequestHandler<GetHealthSummaryQuery, HealthSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetHealthSummaryQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<HealthSummaryDto> Handle(GetHealthSummaryQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId!.Value;

        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        double? latestBmi = null;
        string? bmiLabel = null;
        double? latestWeightKg = null;
        double? latestGlucose = null;
        int? latestSystolic = null;
        int? latestDiastolic = null;
        string? bpLabel = null;
        int activeAlertsCount = 0;
        int vaccinesCompleted = 0;

        if (profile != null)
        {
            var latestMeasurement = await _context.HealthMeasurements
                .AsNoTracking()
                .Where(m => m.HealthProfileId == profile.Id)
                .OrderByDescending(m => m.MeasuredAt)
                .FirstOrDefaultAsync(ct);

            if (latestMeasurement != null)
            {
                latestBmi = latestMeasurement.Bmi.HasValue ? (double)latestMeasurement.Bmi.Value : null;
                latestWeightKg = latestMeasurement.WeightKg.HasValue ? (double)latestMeasurement.WeightKg.Value : null;
                latestGlucose = latestMeasurement.BloodGlucose.HasValue ? (double)latestMeasurement.BloodGlucose.Value : null;
                bmiLabel = latestBmi.HasValue ? HealthCalculations.GetBmiLabel(latestBmi.Value) : null;
            }

            var latestBp = await _context.BloodPressureLogs
                .AsNoTracking()
                .Where(b => b.HealthProfileId == profile.Id)
                .OrderByDescending(b => b.MeasuredAt)
                .FirstOrDefaultAsync(ct);

            if (latestBp != null)
            {
                latestSystolic = latestBp.Systolic;
                latestDiastolic = latestBp.Diastolic;
                bpLabel = HealthCalculations.GetBpLabel(latestBp.Systolic, latestBp.Diastolic);
            }

            activeAlertsCount = await _context.HealthAlerts
                .AsNoTracking()
                .CountAsync(a => a.HealthProfileId == profile.Id && !a.IsAcknowledged, ct);

            vaccinesCompleted = await _context.VaccineRecords
                .AsNoTracking()
                .CountAsync(v => v.HealthProfileId == profile.Id, ct);
        }

        var now = DateTime.UtcNow;
        var upcomingRemindersCount = await _context.Reminders
            .AsNoTracking()
            .CountAsync(r => r.UserId == userId
                && r.Status == "pending"
                && r.RemindAt >= now
                && r.RemindAt <= now.AddDays(7), ct);

        var sevenDaysAgo = now.AddDays(-7);
        var recentLogs = await _context.MedicationLogs
            .AsNoTracking()
            .Where(l => l.ScheduledAt >= sevenDaysAgo
                && l.ScheduledAt <= now
                && (l.Status == MedicationLogStatus.Taken || l.Status == MedicationLogStatus.Skipped))
            .ToListAsync(ct);

        double? complianceRate = null;
        if (recentLogs.Count > 0)
        {
            var taken = recentLogs.Count(l => l.Status == MedicationLogStatus.Taken);
            complianceRate = Math.Round((double)taken / recentLogs.Count * 100, 1);
        }

        return new HealthSummaryDto(
            latestBmi, bmiLabel, latestWeightKg, latestGlucose,
            latestSystolic, latestDiastolic, bpLabel,
            upcomingRemindersCount, activeAlertsCount, vaccinesCompleted, complianceRate
        );
    }

}
