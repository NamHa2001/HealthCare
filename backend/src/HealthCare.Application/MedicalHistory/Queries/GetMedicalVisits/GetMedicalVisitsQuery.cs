using HealthCare.Application.Common.Models;
using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;

namespace HealthCare.Application.MedicalHistory.Queries.GetMedicalVisits;

public record GetMedicalVisitsQuery : IRequest<PagedResult<MedicalVisitListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? Year { get; init; }
    public int? Month { get; init; }
}
