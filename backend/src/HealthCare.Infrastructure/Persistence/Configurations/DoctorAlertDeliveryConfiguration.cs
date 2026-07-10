using HealthCare.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class DoctorAlertDeliveryConfiguration : IEntityTypeConfiguration<DoctorAlertDelivery>
{
    public void Configure(EntityTypeBuilder<DoctorAlertDelivery> builder)
    {
        builder.ToTable("DoctorAlertDeliveries");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.DoctorUserId, x.HealthAlertId, x.Channel }).IsUnique();
        builder.HasIndex(x => new { x.Channel, x.Status }); // digest job quét pending

        builder.Property(x => x.Channel).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10).IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}
