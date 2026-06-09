using HealthCare.Domain.Enums;

namespace HealthCare.Application.HealthProfiles.DTOs;

public class HealthAlertDto
{
    public Guid Id { get; set; }
    public Guid HealthProfileId { get; set; }
    public Guid? MeasurementId { get; set; }
    public Guid? BpLogId { get; set; }
    public AlertType AlertType { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = null!;
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}