using HealthCare.Application.Vaccines.DTOs;
using MediatR;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineProgress;

public record GetVaccineProgressQuery : IRequest<IReadOnlyList<VaccineProgressDto>>;
