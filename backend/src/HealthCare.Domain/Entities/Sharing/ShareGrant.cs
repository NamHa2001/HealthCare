using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Sharing;

public class ShareGrant : BaseEntity
{
    public Guid HealthProfileId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string Scope { get; private set; } = null!; // JSON array: ["measurements","vaccines",...]
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public int AccessCount { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public string? LastAccessedIp { get; private set; }

    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;

    private ShareGrant() { }

    public static ShareGrant Create(Guid healthProfileId, Guid createdByUserId, string tokenHash, string scopeJson, int ttlHours) =>
        new()
        {
            HealthProfileId = healthProfileId,
            CreatedByUserId = createdByUserId,
            TokenHash = tokenHash,
            Scope = scopeJson,
            ExpiresAt = DateTime.UtcNow.AddHours(ttlHours)
        };

    public void Revoke() => RevokedAt = DateTime.UtcNow;

    public void RecordAccess(string? ip)
    {
        AccessCount++;
        LastAccessedAt = DateTime.UtcNow;
        LastAccessedIp = ip;
    }
}
