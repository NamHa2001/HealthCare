namespace HealthCare.Application.Family.DTOs;

public record FamilyMemberDto(
    Guid Id,
    Guid FamilyGroupId,
    Guid? UserId,
    string FullName,
    DateOnly DateOfBirth,
    string Gender,
    string? Relationship,
    Guid? ManagedBy,
    DateTime CreatedAt);
