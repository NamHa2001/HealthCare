using HealthCare.Application.Notifications.DTOs;
using MediatR;

namespace HealthCare.Application.Notifications.Queries.GetNotificationPreference;

public record GetNotificationPreferenceQuery : IRequest<NotificationPreferenceDto>;
