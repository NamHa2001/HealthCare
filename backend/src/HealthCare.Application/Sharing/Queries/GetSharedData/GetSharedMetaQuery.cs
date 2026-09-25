using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Sharing.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Queries.GetSharedData;

/// <summary>Điểm vào của người được chia sẻ — đây là nơi duy nhất ghi nhận lượt truy cập.</summary>
public record GetSharedMetaQuery(string Token, string? IpAddress) : IRequest<SharedMetaDto>;

public class GetSharedMetaQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetSharedMetaQuery, SharedMetaDto>
{
    public async Task<SharedMetaDto> Handle(GetSharedMetaQuery request, CancellationToken ct)
    {
        var grant = await SharedAccess.GetGrantAsync(db, request.Token, null, request.IpAddress, ct);

        var ownerName = await ResolveOwnerNameAsync(grant.HealthProfileId, ct);
        return new SharedMetaDto(ownerName, SharedAccess.GetScopes(grant), grant.ExpiresAt);
    }

    private async Task<string> ResolveOwnerNameAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await db.HealthProfiles.FirstAsync(p => p.Id == profileId, ct);

        if (profile.UserId is not null)
        {
            var user = await db.Users.FirstAsync(u => u.Id == profile.UserId, ct);
            return $"{user.FirstName} {user.LastName}";
        }

        var member = await db.FamilyMembers.FirstAsync(m => m.Id == profile.FamilyMemberId, ct);
        return member.FullName;
    }
}
