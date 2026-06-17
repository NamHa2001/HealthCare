namespace HealthCare.Application.Vaccines.DTOs;

public record VaccinePassportData(
    string FullName,
    string Email,
    string? BloodType,
    DateTime GeneratedAt,
    IReadOnlyList<VaccineRecordDto> Records);
