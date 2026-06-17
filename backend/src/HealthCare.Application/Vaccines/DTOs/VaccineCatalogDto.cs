namespace HealthCare.Application.Vaccines.DTOs;

public record VaccineCatalogDto(
    Guid Id,
    string Name,
    string? ShortName,
    string? DiseasesCovered,
    int TotalDoses,
    bool IsMandatory,
    int? AgeStartMonths,
    string? Notes,
    IReadOnlyList<VaccineScheduleRuleDto> ScheduleRules);

public record VaccineScheduleRuleDto(
    Guid Id,
    int DoseNumber,
    int? MinAgeMonths,
    int? MaxAgeMonths,
    int? MinIntervalDays,
    int? RecommendedIntervalDays);
