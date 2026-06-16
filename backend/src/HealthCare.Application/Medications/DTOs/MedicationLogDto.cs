namespace HealthCare.Application.Medications.DTOs;

public class MedicationLogDto
{
    public Guid Id { get; init; }
    public Guid MedicationScheduleId { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime? TakenAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? SkipReason { get; init; }
}
