using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Family;

public class FamilyMember : AuditableEntity
{
    public Guid FamilyGroupId { get; private set; }
    public Guid? UserId { get; private set; }
    public string FullName { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }
    public string Gender { get; private set; } = null!;
    public string? Relationship { get; private set; }
    public Guid? ManagedBy { get; private set; }

    private FamilyMember() { }

    public static FamilyMember Create(
        Guid familyGroupId,
        string fullName,
        DateOnly dateOfBirth,
        string gender,
        string? relationship,
        Guid managedBy) =>
        new()
        {
            FamilyGroupId = familyGroupId,
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Relationship = relationship,
            ManagedBy = managedBy,
        };

    public void Update(string fullName, DateOnly dateOfBirth, string gender, string? relationship)
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Relationship = relationship;
        UpdatedAt = DateTime.UtcNow;
    }
}
