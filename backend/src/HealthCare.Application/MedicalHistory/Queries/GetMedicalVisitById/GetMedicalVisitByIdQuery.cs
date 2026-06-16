using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;

namespace HealthCare.Application.MedicalHistory.Queries.GetMedicalVisitById;

public record GetMedicalVisitByIdQuery(Guid Id) : IRequest<MedicalVisitDto>;
