using HealthCare.Domain.Entities.Medications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthCare.Infrastructure.Persistence.Configurations;

public class DrugCatalogConfiguration : IEntityTypeConfiguration<DrugCatalog>
{
    public void Configure(EntityTypeBuilder<DrugCatalog> builder)
    {
        builder.ToTable("DrugCatalog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameGeneric)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NameBrand)
            .HasMaxLength(255);

        builder.Property(x => x.DrugClass)
            .HasMaxLength(100);

        builder.Property(x => x.CommonDosages)
            .HasColumnType("nvarchar(max)");

        // Full-Text Index được tạo trong migration SQL thủ công
        // do EF Core không hỗ trợ Full-Text Index trực tiếp
    }
}
