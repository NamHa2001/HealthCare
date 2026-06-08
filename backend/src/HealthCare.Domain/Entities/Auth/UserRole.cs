using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public static UserRole Create(Guid userId, Guid roleId) =>
        new() { UserId = userId, RoleId = roleId };
}