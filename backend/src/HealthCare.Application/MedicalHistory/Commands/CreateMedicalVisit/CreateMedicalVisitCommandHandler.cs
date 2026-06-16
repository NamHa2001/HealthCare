using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.MedicalHistory.DTOs;
using HealthCare.Domain.Entities.MedicalHistory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.MedicalHistory.Commands.CreateMedicalVisit;

public class CreateMedicalVisitCommandHandler : IRequestHandler<CreateMedicalVisitCommand, MedicalVisitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IMapper _mapper;

    public CreateMedicalVisitCommandHandler(
        IApplicationDbContext context, ICurrentUser currentUser, IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<MedicalVisitDto> Handle(CreateMedicalVisitCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var visit = MedicalVisit.Create(
            profile.Id,
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

        _context.MedicalVisits.Add(visit);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<MedicalVisitDto>(visit);
    }
}
