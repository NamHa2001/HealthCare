using HealthCare.Application.Sharing.DTOs;
using MediatR;

namespace HealthCare.Application.Sharing.Commands.CreateShareGrant;

public record CreateShareGrantCommand(
    Guid HealthProfileId,
    List<string> Scope,
    int TtlHours) : IRequest<CreateShareGrantResultDto>;
