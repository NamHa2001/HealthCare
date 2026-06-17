using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Vaccines;

public class VaccineScheduleRule : BaseEntity
{
    public Guid VaccineCatalogId { get; private set; }
    public int DoseNumber { get; private set; }
    public int? MinAgeMonths { get; private set; }
    public int? MaxAgeMonths { get; private set; }
    public int? MinIntervalDays { get; private set; }
    public int? RecommendedIntervalDays { get; private set; }

    public VaccineCatalog VaccineCatalog { get; private set; } = null!;

    private VaccineScheduleRule() { }

    public static VaccineScheduleRule Create(
        Guid vaccineCatalogId,
        int doseNumber,
        int? minAgeMonths = null,
        int? maxAgeMonths = null,
        int? minIntervalDays = null,
        int? recommendedIntervalDays = null) =>
        new()
        {
            VaccineCatalogId = vaccineCatalogId,
            DoseNumber = doseNumber,
            MinAgeMonths = minAgeMonths,
            MaxAgeMonths = maxAgeMonths,
            MinIntervalDays = minIntervalDays,
            RecommendedIntervalDays = recommendedIntervalDays
        };
}
