using HealthCare.Application.Notifications.DTOs;
using MediatR;

namespace HealthCare.Application.Notifications.Commands.UpdateNotificationPreference;

public record UpdateNotificationPreferenceCommand(
    string Timezone,
    string QuietStartTime,
    string QuietEndTime,
    bool PushEnabled,
    bool EmailEnabled,
    bool VaccineReminder,
    bool MedicationReminder,
    bool FollowupReminder,
    bool HealthAlert) : IRequest<NotificationPreferenceDto>;
