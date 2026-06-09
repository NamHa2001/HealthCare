using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.HealthProfile;

public class HealthAlert : BaseEntity
{
    public Guid HealthProfileId { get; private set; }
    public Guid? MeasurementId { get; private set; }
    public Guid? BpLogId { get; private set; }
    public AlertType AlertType { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string Message { get; private set; } = null!;
    public bool IsAcknowledged { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    public HealthProfile HealthProfile { get; private set; } = null!;

    private HealthAlert() { }

    public static HealthAlert Create(
        Guid healthProfileId,
        AlertType alertType,
        AlertSeverity severity,
        string message,
        Guid? measurementId = null,
        Guid? bpLogId = null) =>
        new()
        {
            HealthProfileId = healthProfileId,
            AlertType = alertType,
            Severity = severity,
            Message = message,
            MeasurementId = measurementId,
            BpLogId = bpLogId
        };

    public void Acknowledge()
    {
        IsAcknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
    }
}