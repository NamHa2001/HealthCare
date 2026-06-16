using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Queries.GetMedicationSchedules;

public class GetMedicationSchedulesQueryHandler(IApplicationDbContext db, ICurrentUser currentUser, IMapper mapper)
    : IRequestHandler<GetMedicationSchedulesQuery, List<MedicationLogDto>>
{
    public async Task<List<MedicationLogDto>> Handle(GetMedicationSchedulesQuery request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dayStart = targetDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        var logs = await db.MedicationLogs
            .Include(l => l.MedicationSchedule)
                .ThenInclude(s => s.Medication)
            .Where(l =>
                l.MedicationSchedule.Medication.HealthProfileId == profile.Id &&
                l.MedicationSchedule.Medication.DeletedAt == null &&
                l.ScheduledAt >= dayStart &&
                l.ScheduledAt < dayEnd)
            .OrderBy(l => l.ScheduledAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<MedicationLogDto>>(logs);
    }
}
