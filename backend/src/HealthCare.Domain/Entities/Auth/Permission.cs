using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Auth;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = null!;       // "health_profile:read"
    public string? Description { get; private set; }

    private Permission() { }

    public static Permission Create(string name, string? description = null) =>
        new() { Name = name, Description = description };
}