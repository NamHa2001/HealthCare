using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Vaccines;

public class VaccineCatalog : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? ShortName { get; private set; }
    public string? DiseasesCovered { get; private set; }
    public int TotalDoses { get; private set; }
    public bool IsMandatory { get; private set; }
    public int? AgeStartMonths { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<VaccineScheduleRule> ScheduleRules => _scheduleRules.AsReadOnly();
    private readonly List<VaccineScheduleRule> _scheduleRules = [];

    private VaccineCatalog() { }

    public static VaccineCatalog Create(
        string name,
        int totalDoses,
        string? shortName = null,
        string? diseasesCovered = null,
        bool isMandatory = false,
        int? ageStartMonths = null,
        string? notes = null) =>
        new()
        {
            Name = name,
            TotalDoses = totalDoses,
            ShortName = shortName,
            DiseasesCovered = diseasesCovered,
            IsMandatory = isMandatory,
            AgeStartMonths = ageStartMonths,
            Notes = notes
        };

    public void Update(string name, int totalDoses, string? shortName, string? diseasesCovered,
        bool isMandatory, int? ageStartMonths, string? notes)
    {
        Name = name;
        TotalDoses = totalDoses;
        ShortName = shortName;
        DiseasesCovered = diseasesCovered;
        IsMandatory = isMandatory;
        AgeStartMonths = ageStartMonths;
        Notes = notes;
    }
}
