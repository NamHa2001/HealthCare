namespace HealthCare.Application.Doctors.DTOs;

public record DoctorProfileDto(
    Guid Id,
    string LicenseNumber,
    string Specialty,
    string Workplace,
    string Status,
    string? RejectReason,
    DateTime? VerifiedAt,
    DateTime CreatedAt);

public record DoctorVerificationListItemDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string LicenseNumber,
    string Specialty,
    string Workplace,
    string Status,
    DateTime CreatedAt);

public record DoctorVerificationDetailDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string LicenseNumber,
    string Specialty,
    string Workplace,
    string Status,
    string? RejectReason,
    DateTime? VerifiedAt,
    DateTime CreatedAt,
    IReadOnlyList<string> LicenseDocUrls); // signed URLs, hết hạn 5 phút

/// <summary>File upload đi kèm command (tách khỏi IFormFile để Application không phụ thuộc ASP.NET).</summary>
public record UploadedFile(Stream Content, string FileName, string ContentType, long SizeBytes);
