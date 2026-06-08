namespace HealthCare.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    public void SoftDelete() => DeletedAt = DateTime.UtcNow;
    public void Restore() => DeletedAt = null;
}
