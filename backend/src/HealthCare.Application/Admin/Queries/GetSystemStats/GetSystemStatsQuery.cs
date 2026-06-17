using HealthCare.Application.Admin.DTOs;
using MediatR;

namespace HealthCare.Application.Admin.Queries.GetSystemStats;

public record GetSystemStatsQuery : IRequest<SystemStatsDto>;
