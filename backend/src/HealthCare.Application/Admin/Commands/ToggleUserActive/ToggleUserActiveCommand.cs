using MediatR;

namespace HealthCare.Application.Admin.Commands.ToggleUserActive;

public record ToggleUserActiveCommand(Guid UserId) : IRequest;
