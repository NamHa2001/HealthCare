using AutoMapper;
using AutoMapper.QueryableExtensions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Queries.GetMeasurements;

public class GetMeasurementsQueryHandler
    : IRequestHandler<GetMeasurementsQuery, PagedResult<HealthMeasurementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetMeasurementsQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<HealthMeasurementDto>> Handle(
        GetMeasurementsQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var query = _context.HealthMeasurements
            .AsNoTracking()
            .Where(m => m.HealthProfileId == profile.Id);

        if (request.From.HasValue)
            query = query.Where(m => m.MeasuredAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(m => m.MeasuredAt <= request.To.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.MeasuredAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<HealthMeasurementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<HealthMeasurementDto>(items, request.Page, request.PageSize, total);
    }
}