using HealthCare.Domain.Entities.Vaccines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class VaccineScheduleRuleConfiguration : IEntityTypeConfiguration<VaccineScheduleRule>
{
    public void Configure(EntityTypeBuilder<VaccineScheduleRule> builder)
    {
        builder.ToTable("VaccineScheduleRules");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.VaccineCatalogId, x.DoseNumber }).IsUnique();
    }
}
