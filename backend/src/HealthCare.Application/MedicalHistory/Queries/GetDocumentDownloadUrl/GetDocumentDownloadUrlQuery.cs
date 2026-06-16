using MediatR;

namespace HealthCare.Application.MedicalHistory.Queries.GetDocumentDownloadUrl;

public record GetDocumentDownloadUrlQuery(Guid DocumentId) : IRequest<string>;
