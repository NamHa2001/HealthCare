using AutoMapper;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Medications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.UpdateMedication;

public class UpdateMedicationCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<UpdateMedicationCommand, MedicationDto>
{
    public async Task<MedicationDto> Handle(UpdateMedicationCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var medication = await db.Medications
            .Include(m => m.Schedules)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Medication", request.Id);

        if (medication.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        medication.Update(
            request.DrugName, request.StartDate,
            request.Strength, request.DosageForm, request.Instructions,
            request.EndDate, request.IsOngoing);

        await db.SaveChangesAsync(cancellationToken);
        return mapper.Map<MedicationDto>(medication);
    }
}
