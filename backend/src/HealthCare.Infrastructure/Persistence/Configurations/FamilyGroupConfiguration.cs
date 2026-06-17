using HealthCare.Domain.Entities.Family;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class FamilyGroupConfiguration : IEntityTypeConfiguration<FamilyGroup>
{
    public void Configure(EntityTypeBuilder<FamilyGroup> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(m => m.FamilyGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Domain.Entities.Auth.User>()
            .WithMany()
            .HasForeignKey(g => g.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
