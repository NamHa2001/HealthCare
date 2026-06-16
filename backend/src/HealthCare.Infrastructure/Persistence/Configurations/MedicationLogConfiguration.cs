using HealthCare.Domain.Entities.Medications;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class MedicationLogConfiguration : IEntityTypeConfiguration<MedicationLog>
{
    public void Configure(EntityTypeBuilder<MedicationLog> builder)
    {
        builder.ToTable("MedicationLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MedicationLogStatus.Pending);

        builder.Property(x => x.SkipReason)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.MedicationScheduleId, x.ScheduledAt });
    }
}
