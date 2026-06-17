using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Reminders.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Reminders.Queries.GetReminders;

public class GetRemindersQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetRemindersQuery, IReadOnlyList<ReminderDto>>
{
    public async Task<IReadOnlyList<ReminderDto>> Handle(GetRemindersQuery request, CancellationToken ct)
    {
        var query = db.Reminders
            .Where(r => r.UserId == currentUser.UserId);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(r => r.Status == request.Status);

        if (request.From.HasValue)
            query = query.Where(r => r.RemindAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(r => r.RemindAt <= request.To.Value);

        var reminders = await query
            .OrderBy(r => r.RemindAt)
            .ToListAsync(ct);

        return reminders.Select(r => new ReminderDto(
            r.Id, r.UserId, r.HealthProfileId, r.ReminderType, r.ReferenceId,
            r.Title, r.Body, r.RemindAt, r.Status, r.SentAt, r.CreatedAt))
            .ToList();
    }
}
