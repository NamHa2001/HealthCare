using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetVaccineProgress;

public record GetVaccineProgressQuery : IRequest<IReadOnlyList<VaccineProgressDto>>;
