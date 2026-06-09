using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class HealthAlertConfiguration : IEntityTypeConfiguration<HealthAlert>
{
    public void Configure(EntityTypeBuilder<HealthAlert> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AlertType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Severity)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(a => a.Message)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasIndex(a => new { a.HealthProfileId, a.IsAcknowledged });
    }
}