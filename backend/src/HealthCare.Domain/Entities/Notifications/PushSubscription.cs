using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Notifications;

public class PushSubscription : BaseEntity
{
    public Guid UserId { get; private set; }
    public string FcmToken { get; private set; } = string.Empty;
    public string? DeviceType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastUsedAt { get; private set; }

    public Auth.User User { get; private set; } = null!;

    private PushSubscription() { }

    public static PushSubscription Create(Guid userId, string fcmToken, string? deviceType = null) =>
        new()
        {
            UserId = userId,
            FcmToken = fcmToken,
            DeviceType = deviceType
        };

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
    public void RecordUsage() => LastUsedAt = DateTime.UtcNow;
}
