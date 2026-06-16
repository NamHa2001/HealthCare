using HealthCare.Domain.Entities.MedicalHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class MedicalVisitConfiguration : IEntityTypeConfiguration<MedicalVisit>
{
    public void Configure(EntityTypeBuilder<MedicalVisit> builder)
    {
        builder.HasKey(v => v.Id);

        // Soft delete global query filter — loại bỏ deleted records khỏi tất cả queries
        builder.HasQueryFilter(v => v.DeletedAt == null);

        builder.Property(v => v.FacilityName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(v => v.DoctorName)
            .HasMaxLength(255);

        builder.Property(v => v.Icd10Code)
            .HasMaxLength(10);

        builder.Property(v => v.Cost)
            .HasColumnType("decimal(12,2)");

        builder.Property(v => v.ChiefComplaint)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(v => v.Diagnosis)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(v => v.Treatment)
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.Notes)
            .HasColumnType("nvarchar(max)");

        // FK → HealthProfile CASCADE delete (xóa profile xóa tất cả visits)
        builder.HasOne(v => v.HealthProfile)
            .WithMany()
            .HasForeignKey(v => v.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.HealthProfileId, v.VisitDate });
    }
}
