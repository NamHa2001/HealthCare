namespace HealthCare.Application.Ocr.DTOs;

public class PrescriptionDataDto
{
    public List<ExtractedDrugDto> Drugs { get; init; } = [];
}

public class ExtractedDrugDto
{
    public string DrugName { get; init; } = string.Empty;
    public string? Strength { get; init; }
    public string? DosageForm { get; init; }
    public string? Dosage { get; init; }       // e.g. "2 viên"
    public string? Frequency { get; init; }    // e.g. "2 lần/ngày"
    public string? Duration { get; init; }     // e.g. "7 ngày"
    public string? Instructions { get; init; } // e.g. "sau ăn"
    public decimal ConfidenceScore { get; init; }
    public Guid? MatchedDrugCatalogId { get; init; }
}
