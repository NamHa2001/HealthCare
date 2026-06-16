using HealthCare.Domain.Entities.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class MedicationScheduleConfiguration : IEntityTypeConfiguration<MedicationSchedule>
{
    public void Configure(EntityTypeBuilder<MedicationSchedule> builder)
    {
        builder.ToTable("MedicationSchedules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledTime)
            .HasColumnType("time");

        builder.Property(x => x.DosageAmount)
            .HasMaxLength(50);

        builder.Property(x => x.ReminderEnabled)
            .HasDefaultValue(true);

        builder.Property(x => x.ReminderMinutesBefore)
            .HasDefaultValue(10);

        builder.HasMany(x => x.Logs)
            .WithOne(l => l.MedicationSchedule)
            .HasForeignKey(l => l.MedicationScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
