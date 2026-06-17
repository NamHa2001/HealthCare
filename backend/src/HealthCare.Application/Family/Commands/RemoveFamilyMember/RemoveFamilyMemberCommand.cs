using MediatR;

namespace HealthCare.Application.Family.Commands.RemoveFamilyMember;

public record RemoveFamilyMemberCommand(Guid MemberId) : IRequest;
