using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetHealthScore;

public class GetHealthScoreQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetHealthScoreQuery, HealthScoreDto>
{
    public async Task<HealthScoreDto> Handle(GetHealthScoreQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        double? bmi = null;
        string? bmiLabel = null;
        (int Systolic, int Diastolic)? bp = null;
        string? bpLabel = null;
        int completedVaccines = 0;
        double? medicationComplianceRate = null;

        if (profile is not null)
        {
            var latestMeasurement = await db.HealthMeasurements
                .AsNoTracking()
                .Where(m => m.HealthProfileId == profile.Id)
                .OrderByDescending(m => m.MeasuredAt)
                .FirstOrDefaultAsync(ct);

            if (latestMeasurement?.Bmi is not null)
            {
                bmi = (double)latestMeasurement.Bmi.Value;
                bmiLabel = HealthCalculations.GetBmiLabel(bmi.Value);
            }

            var latestBp = await db.BloodPressureLogs
                .AsNoTracking()
                .Where(b => b.HealthProfileId == profile.Id)
                .OrderByDescending(b => b.MeasuredAt)
                .FirstOrDefaultAsync(ct);

            if (latestBp is not null)
            {
                bp = (latestBp.Systolic, latestBp.Diastolic);
                bpLabel = HealthCalculations.GetBpLabel(latestBp.Systolic, latestBp.Diastolic);
            }

            completedVaccines = await db.VaccineRecords
                .AsNoTracking()
                .CountAsync(v => v.HealthProfileId == profile.Id, ct);

            var since = DateTime.UtcNow.AddDays(-30);
            var logs = await db.MedicationLogs
                .AsNoTracking()
                .Where(l => l.MedicationSchedule.Medication.HealthProfileId == profile.Id
                    && l.ScheduledAt >= since
                    && (l.Status == MedicationLogStatus.Taken || l.Status == MedicationLogStatus.Skipped))
                .ToListAsync(ct);

            if (logs.Count > 0)
            {
                var taken = logs.Count(l => l.Status == MedicationLogStatus.Taken);
                medicationComplianceRate = (double)taken / logs.Count;
            }
        }

        // Count vaccine catalog total doses
        var totalVaccines = await db.VaccineCatalog
            .AsNoTracking()
            .SumAsync(c => c.TotalDoses, ct);

        var bmiPts  = bmi.HasValue ? HealthCalculations.BmiScore(bmi.Value) : 12;
        var bpPts   = bp.HasValue  ? HealthCalculations.BpScore(bp.Value.Systolic, bp.Value.Diastolic) : 12;
        var vacPts  = HealthCalculations.VaccineScore(completedVaccines, totalVaccines);
        var medPts  = HealthCalculations.MedicationScore(medicationComplianceRate);
        var total   = bmiPts + bpPts + vacPts + medPts;

        var grade = total switch
        {
            >= 85 => "Xuất sắc",
            >= 70 => "Tốt",
            >= 55 => "Trung bình",
            >= 40 => "Cần cải thiện",
            _     => "Kém"
        };

        return new HealthScoreDto(
            TotalScore:              total,
            BmiScore:                bmiPts,
            BpScore:                 bpPts,
            VaccineScore:            vacPts,
            MedicationScore:         medPts,
            BmiLabel:                bmiLabel,
            BpLabel:                 bpLabel,
            CompletedVaccines:       completedVaccines,
            TotalVaccines:           totalVaccines,
            MedicationComplianceRate: medicationComplianceRate,
            Grade:                   grade);
    }
}
