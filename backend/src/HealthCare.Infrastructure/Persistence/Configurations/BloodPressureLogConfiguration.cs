using HealthCare.Domain.Entities.HealthProfile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class BloodPressureLogConfiguration : IEntityTypeConfiguration<BloodPressureLog>
{
    public void Configure(EntityTypeBuilder<BloodPressureLog> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Arm)
            .HasMaxLength(5)
            .HasDefaultValue("left");

        builder.Property(b => b.Position)
            .HasMaxLength(10)
            .HasDefaultValue("sitting");

        builder.Property(b => b.Notes)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(b => new { b.HealthProfileId, b.MeasuredAt });
    }
}
