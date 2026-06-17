using MediatR;

namespace HealthCare.Application.Notifications.Commands.UnsubscribePush;

public record UnsubscribePushCommand(Guid SubscriptionId) : IRequest;
