using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class HealthProfileConfiguration : IEntityTypeConfiguration<HealthProfile>
{
    public void Configure(EntityTypeBuilder<HealthProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.BloodType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Allergies)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.ChronicConditions)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.EmergencyContactName)
            .HasMaxLength(255);

        builder.Property(p => p.EmergencyContactPhone)
            .HasMaxLength(20);

        builder.Property(p => p.InsuranceNumber)
            .HasColumnType("varbinary(max)");

        builder.Property(p => p.PrimaryDoctor)
            .HasMaxLength(255);

        builder.ToTable(t => t.HasCheckConstraint(
            "CHK_HealthProfiles_Owner",
            "([UserId] IS NOT NULL AND [FamilyMemberId] IS NULL) OR ([UserId] IS NULL AND [FamilyMemberId] IS NOT NULL)"));

        builder.HasOne<Domain.Entities.Auth.User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Measurements)
            .WithOne(m => m.HealthProfile)
            .HasForeignKey(m => m.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.BloodPressureLogs)
            .WithOne(b => b.HealthProfile)
            .HasForeignKey(b => b.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Alerts)
            .WithOne(a => a.HealthProfile)
            .HasForeignKey(a => a.HealthProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}