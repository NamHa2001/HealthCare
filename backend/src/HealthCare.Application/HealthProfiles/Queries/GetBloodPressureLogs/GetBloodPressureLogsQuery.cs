using HealthCare.Application.Common.Models;
using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;

namespace HealthCare.Application.HealthProfiles.Queries.GetBloodPressureLogs;

public record GetBloodPressureLogsQuery(
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<BloodPressureLogDto>>;
