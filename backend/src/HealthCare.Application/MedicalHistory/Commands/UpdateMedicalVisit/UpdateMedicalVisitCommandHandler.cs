using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Commands.UpdateMedicalVisit;

public class UpdateMedicalVisitCommandHandler : IRequestHandler<UpdateMedicalVisitCommand, MedicalVisitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public UpdateMedicalVisitCommandHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<MedicalVisitDto> Handle(UpdateMedicalVisitCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var visit = await _context.MedicalVisits
            .Include(v => v.Documents)
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("MedicalVisit", request.Id);

        visit.Update(
            request.VisitDate,
            request.FacilityName,
            request.ChiefComplaint,
            request.Diagnosis,
            request.DoctorName,
            request.Icd10Code,
            request.Treatment,
            request.FollowUpDate,
            request.Cost,
            request.Notes);

        await _context.SaveChangesAsync(ct);

        return _mapper.Map<MedicalVisitDto>(visit);
    }
}
