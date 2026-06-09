using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.HealthProfiles.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Queries.GetHealthProfile;

public class GetHealthProfileQueryHandler : IRequestHandler<GetHealthProfileQuery, HealthProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetHealthProfileQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<HealthProfileDto> Handle(GetHealthProfileQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        return _mapper.Map<HealthProfileDto>(profile);
    }
}
