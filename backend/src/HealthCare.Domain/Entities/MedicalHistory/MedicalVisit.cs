using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.MedicalHistory;

public class MedicalVisit : AuditableEntity
{
    public Guid HealthProfileId { get; private set; }
    public DateOnly VisitDate { get; private set; }
    public string FacilityName { get; private set; } = string.Empty;
    public string? DoctorName { get; private set; }
    public string ChiefComplaint { get; private set; } = string.Empty;
    public string Diagnosis { get; private set; } = string.Empty;
    public string? Icd10Code { get; private set; }
    public string? Treatment { get; private set; }
    public DateOnly? FollowUpDate { get; private set; }
    public decimal? Cost { get; private set; }
    public string? Notes { get; private set; }

    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;

    private readonly List<MedicalDocument> _documents = [];
    public IReadOnlyCollection<MedicalDocument> Documents => _documents.AsReadOnly();

    private MedicalVisit() { }

    public static MedicalVisit Create(
        Guid healthProfileId,
        DateOnly visitDate,
        string facilityName,
        string chiefComplaint,
        string diagnosis,
        string? doctorName = null,
        string? icd10Code = null,
        string? treatment = null,
        DateOnly? followUpDate = null,
        decimal? cost = null,
        string? notes = null) =>
        new()
        {
            HealthProfileId = healthProfileId,
            VisitDate = visitDate,
            FacilityName = facilityName,
            ChiefComplaint = chiefComplaint,
            Diagnosis = diagnosis,
            DoctorName = doctorName,
            Icd10Code = icd10Code,
            Treatment = treatment,
            FollowUpDate = followUpDate,
            Cost = cost,
            Notes = notes
        };

    public void Update(
        DateOnly visitDate,
        string facilityName,
        string chiefComplaint,
        string diagnosis,
        string? doctorName,
        string? icd10Code,
        string? treatment,
        DateOnly? followUpDate,
        decimal? cost,
        string? notes)
    {
        VisitDate = visitDate;
        FacilityName = facilityName;
        ChiefComplaint = chiefComplaint;
        Diagnosis = diagnosis;
        DoctorName = doctorName;
        Icd10Code = icd10Code;
        Treatment = treatment;
        FollowUpDate = followUpDate;
        Cost = cost;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
