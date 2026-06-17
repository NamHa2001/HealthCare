using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Family;

public class FamilyGroup : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public Guid AdminId { get; private set; }

    private readonly List<FamilyMember> _members = [];
    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    private FamilyGroup() { }

    public static FamilyGroup Create(string name, Guid adminId) =>
        new() { Name = name, AdminId = adminId };

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }
}
