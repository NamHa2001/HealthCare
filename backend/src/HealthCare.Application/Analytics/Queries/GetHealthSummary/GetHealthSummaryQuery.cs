using HealthCare.Application.Analytics.DTOs;
using MediatR;

namespace HealthCare.Application.Analytics.Queries.GetHealthSummary;

public record GetHealthSummaryQuery : IRequest<HealthSummaryDto>;
