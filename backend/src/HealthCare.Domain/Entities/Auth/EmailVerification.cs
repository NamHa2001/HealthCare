using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Auth;

public class EmailVerification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public VerificationType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    private EmailVerification() { }

    public static EmailVerification Create(Guid userId, string tokenHash, VerificationType type, int expiryHours = 24) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            Type = type,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };

    public void MarkUsed() => IsUsed = true;
}