using MediatR;

namespace HealthCare.Application.Reminders.Commands.CancelReminder;

public record CancelReminderCommand(Guid Id) : IRequest;
