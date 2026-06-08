using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? CreatedByIp { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, int expiryDays, string? ip = null) =>
        new()
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedByIp = ip
        };

    public void Revoke(string? replacedBy = null)
    {
        IsRevoked = true;
        ReplacedByToken = replacedBy;
    }
}