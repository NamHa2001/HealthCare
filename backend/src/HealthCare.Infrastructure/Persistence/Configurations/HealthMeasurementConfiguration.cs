using HealthCare.Domain.Entities.HealthProfile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class HealthMeasurementConfiguration : IEntityTypeConfiguration<HealthMeasurement>
{
    public void Configure(EntityTypeBuilder<HealthMeasurement> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.WeightKg)
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.HeightCm)
            .HasColumnType("decimal(5,1)");

        builder.Property(m => m.Bmi)
            .HasColumnType("decimal(5,2)")
            .HasComputedColumnSql(@"
                CASE
                    WHEN [HeightCm] > 0 AND [WeightKg] IS NOT NULL AND [HeightCm] IS NOT NULL
                    THEN ROUND(
                        CAST([WeightKg] AS decimal(10,4)) /
                        (CAST([HeightCm] AS decimal(10,4)) / 100 *
                         CAST([HeightCm] AS decimal(10,4)) / 100),
                    2)
                    ELSE NULL
                END", stored: true);

        builder.Property(m => m.BodyTemperature)
            .HasColumnType("decimal(4,1)");

        builder.Property(m => m.BloodGlucose)
            .HasColumnType("decimal(5,2)");

        builder.Property(m => m.Spo2Percent)
            .HasColumnType("decimal(4,1)");

        builder.Property(m => m.Source)
            .HasMaxLength(50)
            .HasDefaultValue("manual");

        builder.HasIndex(m => new { m.HealthProfileId, m.MeasuredAt });
    }
}
