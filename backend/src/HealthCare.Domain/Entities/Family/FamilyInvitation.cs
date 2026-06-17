using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Family;

public class FamilyInvitation : AuditableEntity
{
    public Guid FamilyGroupId { get; private set; }
    public string InvitedEmail { get; private set; } = null!;
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsAccepted { get; private set; }

    public FamilyGroup FamilyGroup { get; private set; } = null!;

    private FamilyInvitation() { }

    public static FamilyInvitation Create(Guid groupId, string email, string token) =>
        new()
        {
            FamilyGroupId = groupId,
            InvitedEmail = email.ToLowerInvariant(),
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public void Accept()
    {
        IsAccepted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
