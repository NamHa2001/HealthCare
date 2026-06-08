namespace HealthCare.Application.Common.Interfaces;

public interface IVaccineScheduleService
{
    Task<DateTime?> CalculateNextDueDateAsync(Guid vaccineCatalogId, int doseNumber, DateTime lastDoseDate, CancellationToken ct = default);
}