using AutoMapper;
using AutoMapper.QueryableExtensions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Queries.GetMedicalVisits;

public class GetMedicalVisitsQueryHandler
    : IRequestHandler<GetMedicalVisitsQuery, PagedResult<MedicalVisitListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetMedicalVisitsQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<MedicalVisitListDto>> Handle(
        GetMedicalVisitsQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var query = _context.MedicalVisits
            .AsNoTracking()
            .Where(v => v.HealthProfileId == profile.Id);

        if (request.Year.HasValue)
            query = query.Where(v => v.VisitDate.Year == request.Year.Value);

        if (request.Month.HasValue)
            query = query.Where(v => v.VisitDate.Month == request.Month.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.VisitDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<MedicalVisitListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<MedicalVisitListDto>(items, total, request.Page, request.PageSize);
    }
}
