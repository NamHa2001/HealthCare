namespace HealthCare.Application.Sync.DTOs;

public record SyncChangeDto(
    string EntityType,
    string EntityId,
    string Operation,
    object? Data,
    DateTime ServerTimestamp);
