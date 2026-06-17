using HealthCare.Domain.Entities.Vaccines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class VaccineCatalogConfiguration : IEntityTypeConfiguration<VaccineCatalog>
{
    public void Configure(EntityTypeBuilder<VaccineCatalog> builder)
    {
        builder.ToTable("VaccineCatalog");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ShortName)
            .HasMaxLength(50);

        builder.Property(x => x.DiseasesCovered)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Notes)
            .HasColumnType("nvarchar(max)");

        builder.HasMany(x => x.ScheduleRules)
            .WithOne(r => r.VaccineCatalog)
            .HasForeignKey(r => r.VaccineCatalogId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
