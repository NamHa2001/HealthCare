using HealthCare.Domain.Entities.MedicalHistory;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class MedicalDocumentConfiguration : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasQueryFilter(d => d.DeletedAt == null);

        builder.Property(d => d.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.MimeType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.ThumbnailKey)
            .HasMaxLength(500);

        builder.Property(d => d.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.OcrStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(OcrStatus.Pending);

        builder.Property(d => d.OcrText)
            .HasColumnType("nvarchar(max)");

        // FK → MedicalVisit ON DELETE SET NULL (per SRS §6.3)
        builder.HasOne(d => d.MedicalVisit)
            .WithMany(v => v.Documents)
            .HasForeignKey(d => d.MedicalVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK → HealthProfile NO ACTION (tránh multiple cascade paths)
        builder.HasOne(d => d.HealthProfile)
            .WithMany()
            .HasForeignKey(d => d.HealthProfileId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK → User (người upload) NO ACTION
        builder.HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(d => new { d.HealthProfileId, d.CreatedAt });
    }
}
