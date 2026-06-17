using HealthCare.Application.Reminders.DTOs;
using HealthCare.Domain.Enums;
using MediatR;

namespace HealthCare.Application.Reminders.Commands.CreateReminder;

public record CreateReminderCommand(
    ReminderType ReminderType,
    string Title,
    DateTime RemindAt,
    string? Body = null,
    Guid? ReferenceId = null) : IRequest<ReminderDto>;
