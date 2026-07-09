using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Sharing.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Queries.GetMyShareGrants;

public record GetMyShareGrantsQuery(Guid HealthProfileId) : IRequest<IReadOnlyList<ShareGrantDto>>;

public class GetMyShareGrantsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyShareGrantsQuery, IReadOnlyList<ShareGrantDto>>
{
    public async Task<IReadOnlyList<ShareGrantDto>> Handle(GetMyShareGrantsQuery request, CancellationToken ct)
    {
        await ProfileOwnership.EnsureManagedByAsync(db, request.HealthProfileId, currentUser.UserId!.Value, ct);

        var grants = await db.ShareGrants
            .Where(g => g.HealthProfileId == request.HealthProfileId)
            .OrderByDescending(g => g.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        return grants.Select(g => new ShareGrantDto(
            g.Id, g.HealthProfileId, SharedAccess.GetScopes(g), g.ExpiresAt, g.RevokedAt,
            g.AccessCount, g.LastAccessedAt, g.IsActive, g.CreatedAt))
            .ToList();
    }
}
