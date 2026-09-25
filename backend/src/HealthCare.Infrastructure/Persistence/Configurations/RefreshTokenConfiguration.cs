using HealthCare.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.Property(rt => rt.CreatedByIp).HasMaxLength(45);
        builder.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(64);

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
