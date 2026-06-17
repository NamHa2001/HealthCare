using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Vaccines.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Queries.GetVaccineCatalog;

public class GetVaccineCatalogQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetVaccineCatalogQuery, IReadOnlyList<VaccineCatalogDto>>
{
    public async Task<IReadOnlyList<VaccineCatalogDto>> Handle(GetVaccineCatalogQuery request, CancellationToken ct)
    {
        var catalogs = await db.VaccineCatalog
            .Include(v => v.ScheduleRules)
            .OrderBy(v => v.IsMandatory ? 0 : 1)
            .ThenBy(v => v.AgeStartMonths)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);

        return catalogs.Select(v => new VaccineCatalogDto(
            v.Id, v.Name, v.ShortName, v.DiseasesCovered, v.TotalDoses,
            v.IsMandatory, v.AgeStartMonths, v.Notes,
            v.ScheduleRules
                .OrderBy(r => r.DoseNumber)
                .Select(r => new VaccineScheduleRuleDto(
                    r.Id, r.DoseNumber, r.MinAgeMonths, r.MaxAgeMonths,
                    r.MinIntervalDays, r.RecommendedIntervalDays))
                .ToList()
        )).ToList();
    }
}
