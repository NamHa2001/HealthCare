using HealthCare.Application.HealthProfiles.DTOs;

namespace HealthCare.Application.Doctors.DTOs;

/// <summary>Một dòng trong danh sách bệnh nhân của bác sĩ — chỉ chứa dữ liệu trong ConsentScope.</summary>
public record PatientListItemDto(
    Guid LinkId,
    Guid HealthProfileId,
    string PatientName,
    IReadOnlyList<string> ConsentScope,
    DateTime? ConsentAt,
    int CriticalAlerts,
    int WarningAlerts,
    decimal? LatestBmi,
    string? LatestBp,            // "145/92"
    DateTime? LatestMeasuredAt,
    DateOnly? LastVisitDate);

public record PatientSummaryDto(
    Guid HealthProfileId,
    string PatientName,
    IReadOnlyList<string> ConsentScope,
    DateTime? ConsentAt,
    // Chỉ có giá trị khi scope chứa 'profile'
    string? DateOfBirth,
    string? Gender,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    // Chỉ trả khi scope chứa measurements/blood_pressure
    IReadOnlyList<HealthAlertDto> ActiveAlerts);
