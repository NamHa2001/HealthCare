using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Notifications;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Timezone { get; private set; } = "Asia/Ho_Chi_Minh";
    public TimeOnly QuietStartTime { get; private set; } = new TimeOnly(22, 0);
    public TimeOnly QuietEndTime { get; private set; } = new TimeOnly(7, 0);
    public bool PushEnabled { get; private set; } = true;
    public bool EmailEnabled { get; private set; } = true;
    public bool VaccineReminder { get; private set; } = true;
    public bool MedicationReminder { get; private set; } = true;
    public bool FollowupReminder { get; private set; } = true;
    public bool HealthAlert { get; private set; } = true;

    // DOCTOR_PORTAL.md §7.3 — mặc định bật, khuyến cáo không tắt (nhưng bác sĩ vẫn tự quyết định được)
    public bool DoctorCriticalAlert { get; private set; } = true;
    public bool DoctorDailyDigest { get; private set; } = true;

    public Auth.User User { get; private set; } = null!;

    private NotificationPreference() { }

    public static NotificationPreference Create(Guid userId) =>
        new() { UserId = userId };

    public void Update(
        string timezone,
        TimeOnly quietStartTime,
        TimeOnly quietEndTime,
        bool pushEnabled,
        bool emailEnabled,
        bool vaccineReminder,
        bool medicationReminder,
        bool followupReminder,
        bool healthAlert,
        bool doctorCriticalAlert,
        bool doctorDailyDigest)
    {
        Timezone = timezone;
        QuietStartTime = quietStartTime;
        QuietEndTime = quietEndTime;
        PushEnabled = pushEnabled;
        EmailEnabled = emailEnabled;
        VaccineReminder = vaccineReminder;
        MedicationReminder = medicationReminder;
        FollowupReminder = followupReminder;
        HealthAlert = healthAlert;
        DoctorCriticalAlert = doctorCriticalAlert;
        DoctorDailyDigest = doctorDailyDigest;
    }

    public bool IsInQuietHours(TimeOnly currentTime)
    {
        if (QuietStartTime < QuietEndTime)
            return currentTime >= QuietStartTime && currentTime < QuietEndTime;
        return currentTime >= QuietStartTime || currentTime < QuietEndTime;
    }
}
