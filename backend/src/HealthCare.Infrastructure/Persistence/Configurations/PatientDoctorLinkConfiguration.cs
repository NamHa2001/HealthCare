using HealthCare.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class PatientDoctorLinkConfiguration : IEntityTypeConfiguration<PatientDoctorLink>
{
    public void Configure(EntityTypeBuilder<PatientDoctorLink> builder)
    {
        builder.ToTable("PatientDoctorLinks");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.DoctorUserId, x.HealthProfileId }).IsUnique();
        builder.HasIndex(x => new { x.DoctorUserId, x.Status });
        builder.HasIndex(x => new { x.HealthProfileId, x.Status });

        builder.Property(x => x.InitiatedBy)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.ConsentScope).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ConsentText).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ConsentIp).HasMaxLength(45);
        builder.Property(x => x.ConsentUserAgent).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RevokedBy).HasMaxLength(10);

        builder.HasOne(x => x.HealthProfile)
            .WithMany()
            .HasForeignKey(x => x.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction tránh multiple cascade paths (Users → HealthProfiles → PatientDoctorLinks)
        builder.HasOne(x => x.DoctorUser)
            .WithMany()
            .HasForeignKey(x => x.DoctorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.DomainEvents);
    }
}
