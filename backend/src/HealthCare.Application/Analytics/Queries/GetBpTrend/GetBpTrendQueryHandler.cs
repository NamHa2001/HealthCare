using HealthCare.Application.Analytics.DTOs;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Analytics.Queries.GetBpTrend;

public class GetBpTrendQueryHandler : IRequestHandler<GetBpTrendQuery, List<BpTrendPointDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetBpTrendQueryHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BpTrendPointDto>> Handle(GetBpTrendQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId!.Value;
        var days = Math.Clamp(request.Days, 7, 365);
        var from = DateTime.UtcNow.AddDays(-days);

        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null) return [];

        var logs = await _context.BloodPressureLogs
            .AsNoTracking()
            .Where(b => b.HealthProfileId == profile.Id && b.MeasuredAt >= from)
            .OrderBy(b => b.MeasuredAt)
            .Select(b => new { b.MeasuredAt, b.Systolic, b.Diastolic, b.Pulse })
            .ToListAsync(ct);

        return logs
            .Select(b => new BpTrendPointDto(
                DateOnly.FromDateTime(b.MeasuredAt),
                b.Systolic, b.Diastolic, b.Pulse))
            .ToList();
    }
}
