namespace HealthCare.Application.Vaccines.DTOs;

public record VaccineRecordDto(
    Guid Id,
    Guid HealthProfileId,
    Guid? VaccineCatalogId,
    string VaccineName,
    int DoseNumber,
    DateOnly InjectionDate,
    DateOnly? NextDueDate,
    string? Facility,
    string? LotNumber,
    string? AdministeredBy,
    string? Reaction,
    bool IsOverdue,
    string Status,
    DateTime CreatedAt);
