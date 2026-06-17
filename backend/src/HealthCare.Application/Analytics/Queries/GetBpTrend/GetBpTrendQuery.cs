using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetBpTrend;

public record GetBpTrendQuery(int Days = 30) : IRequest<List<BpTrendPointDto>>;
