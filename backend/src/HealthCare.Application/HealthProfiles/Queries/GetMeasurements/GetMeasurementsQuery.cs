using HealthCare.Application.Common.Models;
using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;

namespace HealthCare.Application.HealthProfiles.Queries.GetMeasurements;

public record GetMeasurementsQuery(
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<HealthMeasurementDto>>;
