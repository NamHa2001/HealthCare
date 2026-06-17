using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Reminders.DTOs;
using HealthCare.Domain.Entities.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Reminders.Commands.CreateReminder;

public class CreateReminderCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateReminderCommand, ReminderDto>
{
    public async Task<ReminderDto> Handle(CreateReminderCommand request, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", currentUser.UserId);

        var reminder = Reminder.Create(
            currentUser.UserId!.Value,
            profile.Id,
            request.ReminderType,
            request.Title,
            request.RemindAt,
            request.ReferenceId,
            request.Body);

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync(ct);

        return new ReminderDto(
            reminder.Id, reminder.UserId, reminder.HealthProfileId, reminder.ReminderType,
            reminder.ReferenceId, reminder.Title, reminder.Body, reminder.RemindAt,
            reminder.Status, reminder.SentAt, reminder.CreatedAt);
    }
}
