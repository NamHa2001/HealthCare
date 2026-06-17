namespace HealthCare.Application.Analytics.DTOs;

public record VaccineProgressDto(
    string VaccineName,
    int TotalDoses,
    int CompletedDoses,
    double Percentage);
