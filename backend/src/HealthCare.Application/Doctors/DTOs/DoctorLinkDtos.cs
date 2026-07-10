namespace HealthCare.Application.Doctors.DTOs;

/// <summary>Liên kết nhìn từ phía bệnh nhân.</summary>
public record DoctorLinkDto(
    Guid Id,
    Guid DoctorUserId,
    string DoctorName,
    string Specialty,
    string Workplace,
    Guid HealthProfileId,
    string ProfileOwnerName,
    string InitiatedBy,
    string Status,
    DateTime? ConsentAt,
    IReadOnlyList<string> ConsentScope,
    string? ConsentText,
    DateTime CreatedAt);

/// <summary>Liên kết nhìn từ phía bác sĩ.</summary>
public record PatientLinkDto(
    Guid Id,
    Guid HealthProfileId,
    string PatientName,
    string InitiatedBy,
    string Status,
    DateTime? ConsentAt,
    IReadOnlyList<string> ConsentScope,
    DateTime CreatedAt);

public record DoctorSearchResultDto(
    Guid UserId,
    string FullName,
    string Specialty,
    string Workplace,
    string LicenseNumber);
