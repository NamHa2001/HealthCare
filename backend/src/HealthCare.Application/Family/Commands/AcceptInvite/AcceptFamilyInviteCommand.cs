using MediatR;

namespace HealthCare.Application.Family.Commands.AcceptInvite;

public record AcceptFamilyInviteCommand(string Token) : IRequest;
