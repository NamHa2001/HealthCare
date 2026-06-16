using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Commands.AddMedicationSchedule;

public record AddMedicationScheduleCommand : IRequest<MedicationScheduleDto>
{
    public Guid MedicationId { get; init; }
    public TimeOnly ScheduledTime { get; init; }
    public string? DosageAmount { get; init; }
    public bool ReminderEnabled { get; init; }
    public int ReminderMinutesBefore { get; init; }
}
