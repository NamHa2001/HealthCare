using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Medications;

public class DrugCatalog : BaseEntity
{
    public string NameGeneric { get; private set; } = string.Empty;
    public string? NameBrand { get; private set; }
    public string? DrugClass { get; private set; }
    public string? CommonDosages { get; private set; } // JSON array

    private DrugCatalog() { }

    public static DrugCatalog Create(
        string nameGeneric,
        string? nameBrand = null,
        string? drugClass = null,
        string? commonDosages = null) =>
        new()
        {
            NameGeneric = nameGeneric,
            NameBrand = nameBrand,
            DrugClass = drugClass,
            CommonDosages = commonDosages
        };
}
