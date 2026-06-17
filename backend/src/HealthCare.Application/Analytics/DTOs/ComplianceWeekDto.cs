namespace HealthCare.Application.Analytics.DTOs;

public record ComplianceWeekDto(
    string WeekLabel,
    double ComplianceRate,
    int Total,
    int Taken);
