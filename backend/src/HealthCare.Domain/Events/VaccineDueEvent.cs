using HealthCare.Domain.Common;

namespace HealthCare.Domain.Events;

public record VaccineDueEvent(
    Guid HealthProfileId,
    Guid UserId,
    Guid VaccineRecordId,
    string VaccineName,
    int DoseNumber,
    DateOnly NextDueDate) : IDomainEvent;
