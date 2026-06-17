using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetVisitFrequency;

public record GetVisitFrequencyQuery(int Months = 6) : IRequest<IReadOnlyList<VisitFrequencyDto>>;
