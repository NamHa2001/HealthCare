using HealthCare.Domain.Entities.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class FamilyInvitationConfiguration : IEntityTypeConfiguration<FamilyInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvitedEmail).IsRequired().HasMaxLength(256);
        builder.Property(i => i.Token).IsRequired().HasMaxLength(128);

        builder.HasIndex(i => i.Token).IsUnique();

        builder.HasOne(i => i.FamilyGroup)
            .WithMany()
            .HasForeignKey(i => i.FamilyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(i => i.DeletedAt == null);
    }
}
