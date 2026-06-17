using HealthCare.Domain.Entities.Vaccines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class VaccineRecordConfiguration : IEntityTypeConfiguration<VaccineRecord>
{
    public void Configure(EntityTypeBuilder<VaccineRecord> builder)
    {
        builder.ToTable("VaccineRecords");
        builder.HasKey(x => x.Id);

        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.Property(x => x.VaccineName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Facility).HasMaxLength(255);
        builder.Property(x => x.LotNumber).HasMaxLength(100);
        builder.Property(x => x.AdministeredBy).HasMaxLength(255);
        builder.Property(x => x.Reaction).HasColumnType("nvarchar(max)");

        builder.Property(x => x.InjectionDate).HasColumnType("date");
        builder.Property(x => x.NextDueDate).HasColumnType("date");

        builder.HasOne(x => x.HealthProfile)
            .WithMany()
            .HasForeignKey(x => x.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.VaccineCatalog)
            .WithMany()
            .HasForeignKey(x => x.VaccineCatalogId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.HealthProfileId, x.InjectionDate });
    }
}
