using HealthCare.Domain.Common;

namespace HealthCare.Domain.Events;

public record MedicationDueEvent(
    Guid HealthProfileId,
    Guid UserId,
    Guid MedicationScheduleId,
    string DrugName,
    TimeOnly ScheduledTime) : IDomainEvent;
