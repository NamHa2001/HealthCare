using HealthCare.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        builder.HasKey(ev => ev.Id);
        builder.Property(ev => ev.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(ev => ev.TokenHash).IsUnique();
        builder.Property(ev => ev.Type).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasOne(ev => ev.User)
            .WithMany()
            .HasForeignKey(ev => ev.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}