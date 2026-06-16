using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Queries.GetMedicalVisitById;

public class GetMedicalVisitByIdQueryHandler : IRequestHandler<GetMedicalVisitByIdQuery, MedicalVisitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public GetMedicalVisitByIdQueryHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<MedicalVisitDto> Handle(GetMedicalVisitByIdQuery request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var visit = await _context.MedicalVisits
            .AsNoTracking()
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("MedicalVisit", request.Id);

        return _mapper.Map<MedicalVisitDto>(visit);
    }
}
