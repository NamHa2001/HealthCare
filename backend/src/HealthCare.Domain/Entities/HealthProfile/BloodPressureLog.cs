using HealthCare.Domain.Common;
using HealthCare.Domain.Events;

namespace HealthCare.Domain.Entities.HealthProfile;

public class BloodPressureLog : BaseEntity
{
    public Guid HealthProfileId { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public int Systolic { get; private set; }
    public int Diastolic { get; private set; }
    public int? Pulse { get; private set; }
    public string Arm { get; private set; } = "left";       // left, right
    public string Position { get; private set; } = "sitting"; // sitting, standing, lying
    public string? Notes { get; private set; }

    public HealthProfile HealthProfile { get; private set; } = null!;

    private BloodPressureLog() { }

    public static BloodPressureLog Create(
        Guid healthProfileId,
        DateTime measuredAt,
        int systolic,
        int diastolic,
        int? pulse = null,
        string arm = "left",
        string position = "sitting",
        string? notes = null)
    {
        var log = new BloodPressureLog
        {
            HealthProfileId = healthProfileId,
            MeasuredAt = measuredAt,
            Systolic = systolic,
            Diastolic = diastolic,
            Pulse = pulse,
            Arm = arm,
            Position = position,
            Notes = notes
        };

        // Tự động raise event nếu vượt ngưỡng (SRS: >140 systolic hoặc >90 diastolic)
        if (systolic > 140 || diastolic > 90)
            log.AddDomainEvent(new BloodPressureAlertEvent(healthProfileId, log.Id, systolic, diastolic));

        return log;
    }
}
