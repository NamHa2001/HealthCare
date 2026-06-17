namespace HealthCare.Application.Vaccines.DTOs;

public record VaccineProgressDto(
    Guid VaccineCatalogId,
    string VaccineName,
    int TotalDoses,
    int DosesCompleted,
    DateOnly? NextDueDate,
    bool IsOverdue,
    IReadOnlyList<VaccineRecordDto> Records);
