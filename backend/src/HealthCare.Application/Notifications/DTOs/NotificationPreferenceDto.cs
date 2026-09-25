namespace HealthCare.Application.Notifications.DTOs;

public record NotificationPreferenceDto(
    string Timezone,
    string QuietStartTime,
    string QuietEndTime,
    bool PushEnabled,
    bool EmailEnabled,
    bool VaccineReminder,
    bool MedicationReminder,
    bool FollowupReminder,
    bool HealthAlert,
    bool DoctorCriticalAlert,
    bool DoctorDailyDigest);
