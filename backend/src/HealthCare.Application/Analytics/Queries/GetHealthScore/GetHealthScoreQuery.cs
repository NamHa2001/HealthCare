using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetHealthScore;

public record GetHealthScoreQuery : IRequest<HealthScoreDto>;
