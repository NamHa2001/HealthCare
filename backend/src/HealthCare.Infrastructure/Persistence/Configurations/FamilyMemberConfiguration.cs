using HealthCare.Domain.Entities.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Gender)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(m => m.Relationship)
            .HasMaxLength(50);

        builder.HasOne<Domain.Entities.Auth.User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Domain.Entities.Auth.User>()
            .WithMany()
            .HasForeignKey(m => m.ManagedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(m => m.DeletedAt == null);
    }
}
