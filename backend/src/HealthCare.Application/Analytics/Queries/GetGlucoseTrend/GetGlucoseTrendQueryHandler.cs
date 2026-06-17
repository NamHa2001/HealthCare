using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetGlucoseTrend;

public class GetGlucoseTrendQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetGlucoseTrendQuery, IReadOnlyList<TrendPointDto>>
{
    public async Task<IReadOnlyList<TrendPointDto>> Handle(
        GetGlucoseTrendQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var days = Math.Clamp(request.Days, 7, 365);
        var from = DateTime.UtcNow.AddDays(-days);

        var profile = await db.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return [];

        var data = await db.HealthMeasurements
            .AsNoTracking()
            .Where(m => m.HealthProfileId == profile.Id
                && m.BloodGlucose != null
                && m.MeasuredAt >= from)
            .OrderBy(m => m.MeasuredAt)
            .Select(m => new { m.MeasuredAt, Value = (double)m.BloodGlucose!.Value })
            .ToListAsync(ct);

        return data
            .Select(m => new TrendPointDto(DateOnly.FromDateTime(m.MeasuredAt), Math.Round(m.Value, 2)))
            .ToList();
    }
}
