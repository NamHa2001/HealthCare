namespace HealthCare.Application.MedicalHistory.DTOs;

public class MedicalVisitDto
{
    public Guid Id { get; set; }
    public Guid HealthProfileId { get; set; }
    public DateOnly VisitDate { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? Icd10Code { get; set; }
    public string? Treatment { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MedicalDocumentDto> Documents { get; set; } = [];
}
