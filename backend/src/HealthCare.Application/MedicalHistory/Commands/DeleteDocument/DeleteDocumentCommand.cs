using MediatR;

namespace HealthCare.Application.MedicalHistory.Commands.DeleteDocument;

public record DeleteDocumentCommand(Guid Id) : IRequest;
