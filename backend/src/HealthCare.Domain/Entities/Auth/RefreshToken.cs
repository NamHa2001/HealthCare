using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, int expiryDays, string? ip = null) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedByIp = ip
        };

    public void Revoke(string? replacedByHash = null)
    {
        IsRevoked = true;
        ReplacedByTokenHash = replacedByHash;
    }
}