namespace HealthCare.Application.Sync.DTOs;

public record SyncOperationDto(
    string Operation,      // create | update | delete
    string EntityType,     // health_measurement | blood_pressure | medical_visit | vaccine_record
    string EntityId,
    Dictionary<string, object?>? Payload,
    long ClientVersion,
    DateTime ClientTimestamp);
