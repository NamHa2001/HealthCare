using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Notifications.Commands.SubscribePush;

public class SubscribePushCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<SubscribePushCommand, Guid>
{
    public async Task<Guid> Handle(SubscribePushCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        // Upsert: nếu token đã tồn tại thì kích hoạt lại
        var existing = await db.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.FcmToken == request.FcmToken, ct);

        if (existing is not null)
        {
            if (!existing.IsActive)
                existing.Reactivate();
            existing.RecordUsage();
            await db.SaveChangesAsync(ct);
            return existing.Id;
        }

        var sub = PushSubscription.Create(userId, request.FcmToken, request.DeviceType);
        db.PushSubscriptions.Add(sub);
        await db.SaveChangesAsync(ct);
        return sub.Id;
    }
}
