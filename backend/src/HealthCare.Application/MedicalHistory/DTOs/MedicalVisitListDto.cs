namespace HealthCare.Application.MedicalHistory.DTOs;

public class MedicalVisitListDto
{
    public Guid Id { get; set; }
    public DateOnly VisitDate { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
