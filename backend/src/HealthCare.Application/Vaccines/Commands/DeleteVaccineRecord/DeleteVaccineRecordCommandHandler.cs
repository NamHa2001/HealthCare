using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Vaccines.Commands.DeleteVaccineRecord;

public class DeleteVaccineRecordCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<DeleteVaccineRecordCommand>
{
    public async Task Handle(DeleteVaccineRecordCommand request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        var record = await db.VaccineRecords
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.HealthProfileId == profile.Id, ct)
            ?? throw new NotFoundException("VaccineRecord", request.Id);

        record.SoftDelete();
        await db.SaveChangesAsync(ct);
    }
}
