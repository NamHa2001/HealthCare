using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetBmiTrend;

public record GetBmiTrendQuery(int Days = 30) : IRequest<List<TrendPointDto>>;
