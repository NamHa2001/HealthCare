using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetVaccineProgress;

public class GetVaccineProgressQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetVaccineProgressQuery, IReadOnlyList<VaccineProgressDto>>
{
    public async Task<IReadOnlyList<VaccineProgressDto>> Handle(
        GetVaccineProgressQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return [];

        // Lấy số mũi đã tiêm theo từng vaccine
        var records = await db.VaccineRecords
            .AsNoTracking()
            .Where(v => v.HealthProfileId == profile.Id)
            .GroupBy(v => v.VaccineName)
            .Select(g => new { VaccineName = g.Key, Completed = g.Count() })
            .ToListAsync(ct);

        // Lấy tổng số mũi theo lịch chuẩn từ catalog
        var catalogDoses = await db.VaccineScheduleRules
            .AsNoTracking()
            .GroupBy(r => r.VaccineCatalogId)
            .Select(g => new { CatalogId = g.Key, Total = g.Count() })
            .ToListAsync(ct);

        var catalogNames = await db.VaccineCatalog
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.TotalDoses })
            .ToListAsync(ct);

        var result = catalogNames
            .Select(cat =>
            {
                var completed = records.FirstOrDefault(r => r.VaccineName == cat.Name)?.Completed ?? 0;
                var total = cat.TotalDoses > 0 ? cat.TotalDoses : 3;
                var pct = Math.Round(Math.Min((double)completed / total * 100, 100), 1);
                return new VaccineProgressDto(cat.Name, total, completed, pct);
            })
            .OrderByDescending(x => x.CompletedDoses)
            .ToList();

        return result;
    }
}
