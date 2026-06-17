namespace HealthCare.Application.Analytics.DTOs;

public record HealthSummaryDto(
    double? LatestBmi,
    string? BmiLabel,
    double? LatestWeightKg,
    double? LatestGlucose,
    int? LatestSystolic,
    int? LatestDiastolic,
    string? BpLabel,
    int UpcomingRemindersCount,
    int ActiveAlertsCount,
    int VaccinesCompleted,
    double? MedicationComplianceRate
);
