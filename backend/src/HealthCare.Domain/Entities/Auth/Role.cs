using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class Role : BaseEntity
{
    public string Name { get; private set; } = null!;       // "admin", "user", ...
    public string DisplayName { get; private set; } = null!;

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public static Role Create(string name, string displayName) =>
        new() { Name = name, DisplayName = displayName };
}