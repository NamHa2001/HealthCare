using HealthCare.Domain.Entities.Sharing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class ShareGrantConfiguration : IEntityTypeConfiguration<ShareGrant>
{
    public void Configure(EntityTypeBuilder<ShareGrant> builder)
    {
        builder.ToTable("ShareGrants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.Property(x => x.Scope)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.LastAccessedIp)
            .HasMaxLength(45);

        builder.HasIndex(x => new { x.HealthProfileId, x.ExpiresAt })
            .HasFilter("[RevokedAt] IS NULL");

        builder.HasOne(x => x.HealthProfile)
            .WithMany()
            .HasForeignKey(x => x.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction để tránh multiple cascade paths (Users → HealthProfiles → ShareGrants)
        builder.HasOne<HealthCare.Domain.Entities.Auth.User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
