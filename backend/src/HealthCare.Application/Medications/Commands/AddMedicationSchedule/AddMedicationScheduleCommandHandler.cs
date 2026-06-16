using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using HealthCare.Domain.Entities.Medications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.AddMedicationSchedule;

public class AddMedicationScheduleCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<AddMedicationScheduleCommand, MedicationScheduleDto>
{
    public async Task<MedicationScheduleDto> Handle(AddMedicationScheduleCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var medication = await db.Medications
            .FirstOrDefaultAsync(m => m.Id == request.MedicationId, cancellationToken)
            ?? throw new NotFoundException("Medication", request.MedicationId);

        if (medication.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        var schedule = MedicationSchedule.Create(
            medication.Id,
            request.ScheduledTime,
            request.DosageAmount,
            request.ReminderEnabled,
            request.ReminderMinutesBefore);

        db.MedicationSchedules.Add(schedule);
        await db.SaveChangesAsync(cancellationToken);

        return mapper.Map<MedicationScheduleDto>(schedule);
    }
}
