using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Vaccines;

public class VaccineRecord : AuditableEntity
{
    public Guid HealthProfileId { get; private set; }
    public Guid? VaccineCatalogId { get; private set; }
    public string VaccineName { get; private set; } = string.Empty;
    public int DoseNumber { get; private set; }
    public DateOnly InjectionDate { get; private set; }
    public DateOnly? NextDueDate { get; private set; }
    public string? Facility { get; private set; }
    public string? LotNumber { get; private set; }
    public string? AdministeredBy { get; private set; }
    public string? Reaction { get; private set; }
    public Guid? DocumentId { get; private set; }

    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;
    public VaccineCatalog? VaccineCatalog { get; private set; }

    private VaccineRecord() { }

    public static VaccineRecord Create(
        Guid healthProfileId,
        string vaccineName,
        int doseNumber,
        DateOnly injectionDate,
        DateOnly? nextDueDate = null,
        Guid? vaccineCatalogId = null,
        string? facility = null,
        string? lotNumber = null,
        string? administeredBy = null,
        string? reaction = null,
        Guid? documentId = null) =>
        new()
        {
            HealthProfileId = healthProfileId,
            VaccineName = vaccineName,
            DoseNumber = doseNumber,
            InjectionDate = injectionDate,
            NextDueDate = nextDueDate,
            VaccineCatalogId = vaccineCatalogId,
            Facility = facility,
            LotNumber = lotNumber,
            AdministeredBy = administeredBy,
            Reaction = reaction,
            DocumentId = documentId
        };

    public void Update(
        string vaccineName,
        int doseNumber,
        DateOnly injectionDate,
        DateOnly? nextDueDate,
        string? facility,
        string? lotNumber,
        string? administeredBy,
        string? reaction)
    {
        VaccineName = vaccineName;
        DoseNumber = doseNumber;
        InjectionDate = injectionDate;
        NextDueDate = nextDueDate;
        Facility = facility;
        LotNumber = lotNumber;
        AdministeredBy = administeredBy;
        Reaction = reaction;
    }

    public void SetNextDueDate(DateOnly? date) => NextDueDate = date;

    public bool IsOverdue() => NextDueDate.HasValue && NextDueDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
}
