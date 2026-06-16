using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.DeleteMedicationSchedule;

public class DeleteMedicationScheduleCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<DeleteMedicationScheduleCommand>
{
    public async Task Handle(DeleteMedicationScheduleCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var schedule = await db.MedicationSchedules
            .Include(s => s.Medication)
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId, cancellationToken)
            ?? throw new NotFoundException("MedicationSchedule", request.ScheduleId);

        if (schedule.Medication.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        db.MedicationSchedules.Remove(schedule);
        await db.SaveChangesAsync(cancellationToken);
    }
}
