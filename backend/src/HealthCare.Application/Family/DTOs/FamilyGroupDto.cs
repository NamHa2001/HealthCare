namespace HealthCare.Application.Family.DTOs;

public record FamilyGroupDto(
    Guid Id,
    string Name,
    Guid AdminId,
    DateTime CreatedAt,
    IReadOnlyList<FamilyMemberDto> Members);
