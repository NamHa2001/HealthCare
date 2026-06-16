using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Medications;

public class MedicationLog : BaseEntity
{
    public Guid MedicationScheduleId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? TakenAt { get; private set; }
    public MedicationLogStatus Status { get; private set; } = MedicationLogStatus.Pending;
    public string? SkipReason { get; private set; }

    public MedicationSchedule MedicationSchedule { get; private set; } = null!;

    private MedicationLog() { }

    public static MedicationLog Create(Guid medicationScheduleId, DateTime scheduledAt) =>
        new()
        {
            MedicationScheduleId = medicationScheduleId,
            ScheduledAt = scheduledAt,
            Status = MedicationLogStatus.Pending
        };

    public void MarkTaken()
    {
        Status = MedicationLogStatus.Taken;
        TakenAt = DateTime.UtcNow;
    }

    public void MarkSkipped(string? skipReason = null)
    {
        Status = MedicationLogStatus.Skipped;
        SkipReason = skipReason;
    }
}
