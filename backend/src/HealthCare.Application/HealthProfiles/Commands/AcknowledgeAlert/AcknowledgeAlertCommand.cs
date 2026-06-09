using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.AcknowledgeAlert;

public record AcknowledgeAlertCommand(Guid AlertId) : IRequest;