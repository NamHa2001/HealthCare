namespace HealthCare.Application.Admin.DTOs;

public class AuditLogDto
{
    public long Id { get; init; }
    public string EventType { get; init; } = null!;
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public string Resource { get; init; } = null!;
    public string Action { get; init; } = null!;
    public Guid? EntityId { get; init; }
    public string? IpAddress { get; init; }
    public DateTime CreatedAt { get; init; }
}
