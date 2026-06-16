namespace HealthCare.Application.Medications.DTOs;

public class MedicationScheduleDto
{
    public Guid Id { get; init; }
    public Guid MedicationId { get; init; }
    public TimeOnly ScheduledTime { get; init; }
    public string? DosageAmount { get; init; }
    public bool ReminderEnabled { get; init; }
    public int ReminderMinutesBefore { get; init; }
}
