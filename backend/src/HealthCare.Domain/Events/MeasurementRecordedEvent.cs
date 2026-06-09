using HealthCare.Domain.Common;

namespace HealthCare.Domain.Events;

public record MeasurementRecordedEvent(
    Guid HealthProfileId,
    Guid MeasurementId,
    decimal? Bmi,
    decimal? Spo2Percent,
    int? HeartRateBpm,
    decimal? BloodGlucose
) : IDomainEvent;