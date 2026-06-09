using HealthCare.Domain.Common;

namespace HealthCare.Domain.Events;

public record BloodPressureAlertEvent(
    Guid HealthProfileId,
    Guid BpLogId,
    int Systolic,
    int Diastolic
) : IDomainEvent;