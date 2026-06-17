using MediatR;

namespace HealthCare.Application.Notifications.Commands.SubscribePush;

public record SubscribePushCommand(string FcmToken, string? DeviceType) : IRequest<Guid>;
