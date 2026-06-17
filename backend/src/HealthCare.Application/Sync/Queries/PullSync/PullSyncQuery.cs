using HealthCare.Application.Sync.DTOs;
using MediatR;

namespace HealthCare.Application.Sync.Queries.PullSync;

public record PullSyncQuery(long Since) : IRequest<PullSyncResponseDto>;

public record PullSyncResponseDto(IReadOnlyList<SyncChangeDto> Changes, long ServerTimestamp);
