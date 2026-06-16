using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Medications;

public class MedicationSchedule : BaseEntity
{
    public Guid MedicationId { get; private set; }
    public TimeOnly ScheduledTime { get; private set; }
    public string? DosageAmount { get; private set; }
    public bool ReminderEnabled { get; private set; }
    public int ReminderMinutesBefore { get; private set; }

    public Medication Medication { get; private set; } = null!;
    public IReadOnlyCollection<MedicationLog> Logs => _logs.AsReadOnly();
    private readonly List<MedicationLog> _logs = [];

    private MedicationSchedule() { }

    public static MedicationSchedule Create(
        Guid medicationId,
        TimeOnly scheduledTime,
        string? dosageAmount = null,
        bool reminderEnabled = true,
        int reminderMinutesBefore = 10) =>
        new()
        {
            MedicationId = medicationId,
            ScheduledTime = scheduledTime,
            DosageAmount = dosageAmount,
            ReminderEnabled = reminderEnabled,
            ReminderMinutesBefore = reminderMinutesBefore
        };

    public void Update(TimeOnly scheduledTime, string? dosageAmount, bool reminderEnabled, int reminderMinutesBefore)
    {
        ScheduledTime = scheduledTime;
        DosageAmount = dosageAmount;
        ReminderEnabled = reminderEnabled;
        ReminderMinutesBefore = reminderMinutesBefore;
    }
}
