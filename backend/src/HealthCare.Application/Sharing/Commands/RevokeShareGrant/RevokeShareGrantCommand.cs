using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Commands.RevokeShareGrant;

public record RevokeShareGrantCommand(Guid Id) : IRequest;

public class RevokeShareGrantCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<RevokeShareGrantCommand>
{
    public async Task Handle(RevokeShareGrantCommand request, CancellationToken ct)
    {
        var grant = await db.ShareGrants
            .FirstOrDefaultAsync(g => g.Id == request.Id, ct)
            ?? throw new NotFoundException("ShareGrant", request.Id);

        // Người tạo hoặc người quản lý hồ sơ đều được thu hồi
        if (grant.CreatedByUserId != currentUser.UserId)
            await ProfileOwnership.EnsureManagedByAsync(db, grant.HealthProfileId, currentUser.UserId!.Value, ct);

        grant.Revoke();
        await db.SaveChangesAsync(ct);
    }
}
