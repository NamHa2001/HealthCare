using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetBmiTrend;

public class GetBmiTrendQueryHandler : IRequestHandler<GetBmiTrendQuery, List<TrendPointDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetBmiTrendQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TrendPointDto>> Handle(GetBmiTrendQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId!.Value;
        var days = Math.Clamp(request.Days, 7, 365);
        var from = DateTime.UtcNow.AddDays(-days);

        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null) return [];

        var measurements = await _context.HealthMeasurements
            .AsNoTracking()
            .Where(m => m.HealthProfileId == profile.Id
                && m.Bmi != null
                && m.MeasuredAt >= from)
            .OrderBy(m => m.MeasuredAt)
            .Select(m => new { m.MeasuredAt, Bmi = (double)m.Bmi!.Value })
            .ToListAsync(ct);

        return measurements
            .Select(m => new TrendPointDto(DateOnly.FromDateTime(m.MeasuredAt), Math.Round(m.Bmi, 1)))
            .ToList();
    }
}
