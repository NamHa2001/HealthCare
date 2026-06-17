using HealthCare.Application.Reminders.DTOs;
using MediatR;

namespace HealthCare.Application.Reminders.Queries.GetReminders;

public record GetRemindersQuery(
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<IReadOnlyList<ReminderDto>>;
