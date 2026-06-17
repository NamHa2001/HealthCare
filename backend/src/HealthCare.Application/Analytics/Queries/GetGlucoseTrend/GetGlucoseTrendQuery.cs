using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetGlucoseTrend;

public record GetGlucoseTrendQuery(int Days = 30) : IRequest<IReadOnlyList<TrendPointDto>>;
