using HealthCare.Domain.Enums;

namespace HealthCare.Application.Reminders.DTOs;

public record ReminderDto(
    Guid Id,
    Guid UserId,
    Guid HealthProfileId,
    ReminderType ReminderType,
    Guid? ReferenceId,
    string Title,
    string? Body,
    DateTime RemindAt,
    string Status,
    DateTime? SentAt,
    DateTime CreatedAt);
