using HealthCare.Application.Vaccines.DTOs;
using MediatR;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineCatalog;

public record GetVaccineCatalogQuery : IRequest<IReadOnlyList<VaccineCatalogDto>>;
