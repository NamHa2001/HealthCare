using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Queries.GetMedications;

public class GetMedicationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetMedicationsQuery, List<MedicationDto>>
{
    public async Task<List<MedicationDto>> Handle(GetMedicationsQuery request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var query = db.Medications
            .Include(m => m.Schedules)
            .Where(m => m.HealthProfileId == profile.Id);

        if (request.ActiveOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(m => m.IsOngoing || (m.EndDate == null || m.EndDate >= today));
        }

        var medications = await query
            .OrderByDescending(m => m.StartDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<MedicationDto>>(medications);
    }
}
