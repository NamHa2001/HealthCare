using HealthCare.Application.Family.DTOs;
using MediatR;

namespace HealthCare.Application.Family.Commands.AddFamilyMember;

public record AddFamilyMemberCommand(
    string FullName,
    DateOnly DateOfBirth,
    string Gender,
    string? Relationship) : IRequest<FamilyMemberDto>;
