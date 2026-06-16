using HealthCare.Domain.Enums;

namespace HealthCare.Application.MedicalHistory.DTOs;

public class MedicalDocumentDto
{
    public Guid Id { get; set; }
    public Guid? MedicalVisitId { get; set; }
    public Guid HealthProfileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public DocumentType? DocumentType { get; set; }
    public OcrStatus OcrStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
