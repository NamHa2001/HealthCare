using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Medications.Commands.DeleteMedication;

public class DeleteMedicationCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<DeleteMedicationCommand>
{
    public async Task Handle(DeleteMedicationCommand request, CancellationToken cancellationToken)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId!);

        var medication = await db.Medications
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Medication", request.Id);

        if (medication.HealthProfileId != profile.Id)
            throw new ForbiddenException();

        medication.SoftDelete();
        await db.SaveChangesAsync(cancellationToken);
    }
}
