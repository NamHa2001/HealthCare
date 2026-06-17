namespace HealthCare.Application.Common.Interfaces;

public interface IVaccineScheduleService
{
    Task<DateOnly?> CalculateNextDueDateAsync(Guid vaccineCatalogId, int doseNumber, DateOnly lastInjectionDate, CancellationToken ct = default);
}