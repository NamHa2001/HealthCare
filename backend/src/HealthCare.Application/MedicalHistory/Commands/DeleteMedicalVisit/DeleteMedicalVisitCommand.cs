using MediatR;

namespace HealthCare.Application.MedicalHistory.Commands.DeleteMedicalVisit;

public record DeleteMedicalVisitCommand(Guid Id) : IRequest;
