using HealthCare.Application.Sync.DTOs;
using MediatR;

namespace HealthCare.Application.Sync.Commands.PushSync;

public record PushSyncCommand(IReadOnlyList<SyncOperationDto> Operations) : IRequest<PushSyncResultDto>;

public record PushSyncResultDto(int Applied, int Skipped, IReadOnlyList<string> Errors, long ServerTimestamp);
