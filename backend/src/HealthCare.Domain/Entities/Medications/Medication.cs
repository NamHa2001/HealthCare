using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Medications;

public class Medication : AuditableEntity
{
    public Guid HealthProfileId { get; private set; }
    public Guid? MedicalVisitId { get; private set; }
    public Guid? DrugCatalogId { get; private set; }
    public string DrugName { get; private set; } = string.Empty;
    public string? Strength { get; private set; }
    public string? DosageForm { get; private set; }
    public string? Instructions { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsOngoing { get; private set; }
    public Guid? OcrSourceDocId { get; private set; }
    public decimal? ConfidenceScore { get; private set; }

    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;
    public DrugCatalog? DrugCatalog { get; private set; }
    public IReadOnlyCollection<MedicationSchedule> Schedules => _schedules.AsReadOnly();
    private readonly List<MedicationSchedule> _schedules = [];

    private Medication() { }

    public static Medication Create(
        Guid healthProfileId,
        string drugName,
        DateOnly startDate,
        Guid? medicalVisitId = null,
        Guid? drugCatalogId = null,
        string? strength = null,
        string? dosageForm = null,
        string? instructions = null,
        DateOnly? endDate = null,
        bool isOngoing = false,
        Guid? ocrSourceDocId = null,
        decimal? confidenceScore = null) =>
        new()
        {
            HealthProfileId = healthProfileId,
            DrugName = drugName,
            StartDate = startDate,
            MedicalVisitId = medicalVisitId,
            DrugCatalogId = drugCatalogId,
            Strength = strength,
            DosageForm = dosageForm,
            Instructions = instructions,
            EndDate = endDate,
            IsOngoing = isOngoing,
            OcrSourceDocId = ocrSourceDocId,
            ConfidenceScore = confidenceScore
        };

    public void Update(
        string drugName,
        DateOnly startDate,
        string? strength = null,
        string? dosageForm = null,
        string? instructions = null,
        DateOnly? endDate = null,
        bool isOngoing = false)
    {
        DrugName = drugName;
        StartDate = startDate;
        Strength = strength;
        DosageForm = dosageForm;
        Instructions = instructions;
        EndDate = endDate;
        IsOngoing = isOngoing;
    }
}
