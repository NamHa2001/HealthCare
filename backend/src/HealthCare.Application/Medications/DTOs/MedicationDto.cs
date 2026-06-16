namespace HealthCare.Application.Medications.DTOs;

public class MedicationDto
{
    public Guid Id { get; init; }
    public Guid HealthProfileId { get; init; }
    public Guid? MedicalVisitId { get; init; }
    public Guid? DrugCatalogId { get; init; }
    public string DrugName { get; init; } = string.Empty;
    public string? Strength { get; init; }
    public string? DosageForm { get; init; }
    public string? Instructions { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsOngoing { get; init; }
    public Guid? OcrSourceDocId { get; init; }
    public decimal? ConfidenceScore { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<MedicationScheduleDto> Schedules { get; init; } = [];
}
