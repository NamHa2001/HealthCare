namespace HealthCare.Application.Analytics.DTOs;

public record HealthScoreDto(
    int TotalScore,
    int BmiScore,
    int BpScore,
    int VaccineScore,
    int MedicationScore,
    string? BmiLabel,
    string? BpLabel,
    int CompletedVaccines,
    int TotalVaccines,
    double? MedicationComplianceRate,
    string Grade);
