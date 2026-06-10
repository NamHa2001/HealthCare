using AutoMapper;
using AutoMapper.QueryableExtensions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Queries.GetBloodPressureLogs;

public class GetBloodPressureLogsQueryHandler
    : IRequestHandler<GetBloodPressureLogsQuery, PagedResult<BloodPressureLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetBloodPressureLogsQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<BloodPressureLogDto>> Handle(
        GetBloodPressureLogsQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var query = _context.BloodPressureLogs
            .AsNoTracking()
            .Where(b => b.HealthProfileId == profile.Id);

        if (request.From.HasValue)
            query = query.Where(b => b.MeasuredAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(b => b.MeasuredAt <= request.To.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(b => b.MeasuredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<BloodPressureLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<BloodPressureLogDto>(items, total, request.Page, request.PageSize);
    }
}