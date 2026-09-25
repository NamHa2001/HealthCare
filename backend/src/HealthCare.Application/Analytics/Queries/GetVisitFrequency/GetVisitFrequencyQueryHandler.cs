using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetVisitFrequency;

public class GetVisitFrequencyQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetVisitFrequencyQuery, IReadOnlyList<VisitFrequencyDto>>
{
    public async Task<IReadOnlyList<VisitFrequencyDto>> Handle(
        GetVisitFrequencyQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var months = Math.Clamp(request.Months, 1, 24);
        var from = DateTime.UtcNow.AddMonths(-months);

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return [];

        var visits = await db.MedicalVisits
            .AsNoTracking()
            .Where(v => v.HealthProfileId == profile.Id
                && v.VisitDate >= DateOnly.FromDateTime(from))
            .Select(v => v.VisitDate)
            .ToListAsync(ct);

        var grouped = visits
            .GroupBy(d => new { d.Year, d.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new VisitFrequencyDto(
                $"{g.Key.Year}-{g.Key.Month:D2}",
                g.Count()))
            .ToList();

        return grouped;
    }
}
