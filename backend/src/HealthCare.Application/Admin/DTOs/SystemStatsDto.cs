namespace HealthCare.Application.Admin.DTOs;

public record SystemStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalHealthProfiles,
    int TotalMeasurements,
    int TotalBpLogs,
    int TotalVaccineRecords,
    int TotalMedications,
    int TotalMedicalVisits,
    int TotalReminders
);
