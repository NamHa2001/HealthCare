namespace HealthCare.Domain.Entities.Audit;

public class AuditLog
{
    public long Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? OldValueHash { get; private set; }
    public string? NewValueHash { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        string eventType,
        string resource,
        string action,
        Guid? userId = null,
        Guid? targetUserId = null,
        Guid? entityId = null,
        string? oldValueHash = null,
        string? newValueHash = null,
        string? ipAddress = null,
        string? userAgent = null) =>
        new()
        {
            EventType = eventType,
            Resource = resource,
            Action = action,
            UserId = userId,
            TargetUserId = targetUserId,
            EntityId = entityId,
            OldValueHash = oldValueHash,
            NewValueHash = newValueHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
}
