using HealthCare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Services.Vaccines;

public class VaccineScheduleService(IApplicationDbContext db) : IVaccineScheduleService
{
    public async Task<DateOnly?> CalculateNextDueDateAsync(
        Guid vaccineCatalogId,
        int currentDoseNumber,
        DateOnly lastInjectionDate,
        CancellationToken ct = default)
    {
        var catalog = await db.VaccineCatalog
            .Include(v => v.ScheduleRules)
            .FirstOrDefaultAsync(v => v.Id == vaccineCatalogId, ct);

        if (catalog == null) return null;

        var nextDoseNumber = currentDoseNumber + 1;
        if (nextDoseNumber > catalog.TotalDoses) return null;

        var nextRule = catalog.ScheduleRules
            .FirstOrDefault(r => r.DoseNumber == nextDoseNumber);

        if (nextRule == null) return null;

        var intervalDays = nextRule.RecommendedIntervalDays
            ?? nextRule.MinIntervalDays
            ?? 30;

        var nextDueDate = lastInjectionDate.AddDays(intervalDays);

        if (nextDueDate < DateOnly.FromDateTime(DateTime.UtcNow))
            nextDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        return nextDueDate;
    }
}
