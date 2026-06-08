using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class EmailVerification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    private EmailVerification() { }

    public static EmailVerification Create(Guid userId, string token, int expiryHours = 24) =>
        new()
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours)
        };

    public void MarkUsed() => IsUsed = true;
}