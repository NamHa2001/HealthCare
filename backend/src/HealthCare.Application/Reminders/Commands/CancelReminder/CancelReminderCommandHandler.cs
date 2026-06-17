using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Reminders.Commands.CancelReminder;

public class CancelReminderCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CancelReminderCommand>
{
    public async Task Handle(CancelReminderCommand request, CancellationToken ct)
    {
        var reminder = await db.Reminders
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("Reminder", request.Id);

        reminder.Cancel();
        await db.SaveChangesAsync(ct);
    }
}
