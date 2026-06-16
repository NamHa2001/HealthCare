namespace HealthCare.Application.Ocr.DTOs;

public class OcrResultDto
{
    public Guid DocumentId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? RawText { get; init; }
    public decimal? ConfidenceScore { get; init; }
    public PrescriptionDataDto? Prescription { get; init; }
}
