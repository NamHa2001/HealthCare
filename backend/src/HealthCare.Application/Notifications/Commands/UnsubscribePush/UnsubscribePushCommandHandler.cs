using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Notifications.Commands.UnsubscribePush;

public class UnsubscribePushCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UnsubscribePushCommand>
{
    public async Task Handle(UnsubscribePushCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var sub = await db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("PushSubscription", request.SubscriptionId);

        sub.Deactivate();
        await db.SaveChangesAsync(ct);
    }
}
