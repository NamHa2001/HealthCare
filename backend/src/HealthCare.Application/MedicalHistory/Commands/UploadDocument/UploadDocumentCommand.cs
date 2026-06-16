using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Domain.Enums;
using MediatR;

namespace HealthCare.Application.MedicalHistory.Commands.UploadDocument;

public record UploadDocumentCommand : IRequest<MedicalDocumentDto>
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public Guid? MedicalVisitId { get; init; }
    public DocumentType? DocumentType { get; init; }
}
