using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Notifications;

public class Reminder : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid HealthProfileId { get; private set; }
    public ReminderType ReminderType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Body { get; private set; }
    public DateTime RemindAt { get; private set; }
    public string Status { get; private set; } = "pending";
    public DateTime? SentAt { get; private set; }

    public Auth.User User { get; private set; } = null!;

    private Reminder() { }

    public static Reminder Create(
        Guid userId,
        Guid healthProfileId,
        ReminderType reminderType,
        string title,
        DateTime remindAt,
        Guid? referenceId = null,
        string? body = null) =>
        new()
        {
            UserId = userId,
            HealthProfileId = healthProfileId,
            ReminderType = reminderType,
            Title = title,
            RemindAt = remindAt,
            ReferenceId = referenceId,
            Body = body
        };

    public void MarkSent()
    {
        Status = "sent";
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = "failed";
    public void Cancel() => Status = "cancelled";

    public void Update(string title, string? body, DateTime remindAt)
    {
        Title = title;
        Body = body;
        RemindAt = remindAt;
    }
}
