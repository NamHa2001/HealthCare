namespace HealthCare.Application.Sharing.DTOs;

public record ShareGrantDto(
    Guid Id,
    Guid HealthProfileId,
    IReadOnlyList<string> Scope,
    DateTime ExpiresAt,
    DateTime? RevokedAt,
    int AccessCount,
    DateTime? LastAccessedAt,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>Trả về duy nhất 1 lần khi tạo — token gốc không lưu ở đâu khác.</summary>
public record CreateShareGrantResultDto(
    Guid Id,
    string Token,
    IReadOnlyList<string> Scope,
    DateTime ExpiresAt);

public record SharedMetaDto(
    string OwnerName,
    IReadOnlyList<string> Scope,
    DateTime ExpiresAt);

/// <summary>Hồ sơ tĩnh read-only cho người được chia sẻ — KHÔNG bao gồm số BHYT.</summary>
public record SharedProfileDto(
    string OwnerName,
    string? DateOfBirth,
    string? Gender,
    string? BloodType,
    string? Allergies,
    string? ChronicConditions,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? PrimaryDoctor);
