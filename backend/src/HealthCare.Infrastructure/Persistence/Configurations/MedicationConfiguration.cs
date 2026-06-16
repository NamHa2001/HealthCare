using HealthCare.Domain.Entities.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("Medications");

        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.Property(x => x.DrugName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Strength)
            .HasMaxLength(100);

        builder.Property(x => x.DosageForm)
            .HasMaxLength(100);

        builder.Property(x => x.Instructions)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.StartDate)
            .HasColumnType("date");

        builder.Property(x => x.EndDate)
            .HasColumnType("date");

        builder.Property(x => x.ConfidenceScore)
            .HasColumnType("decimal(3,2)");

        builder.HasOne(x => x.HealthProfile)
            .WithMany()
            .HasForeignKey(x => x.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DrugCatalog)
            .WithMany()
            .HasForeignKey(x => x.DrugCatalogId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Schedules)
            .WithOne(s => s.Medication)
            .HasForeignKey(s => s.MedicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.HealthProfileId, x.StartDate });
    }
}
