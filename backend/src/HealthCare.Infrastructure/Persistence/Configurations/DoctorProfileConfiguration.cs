using HealthCare.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.ToTable("DoctorProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicenseNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.Specialty)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Workplace)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.LicenseDocKeys)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.RejectReason)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<DoctorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);
    }
}
