using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Sharing.Common;
using HealthCare.Application.Sharing.DTOs;
using HealthCare.Domain.Entities.Sharing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Commands.CreateShareGrant;

public class CreateShareGrantCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateShareGrantCommand, CreateShareGrantResultDto>
{
    private const int MaxActiveGrantsPerProfile = 3;

    public async Task<CreateShareGrantResultDto> Handle(CreateShareGrantCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        await ProfileOwnership.EnsureManagedByAsync(db, request.HealthProfileId, userId, ct);

        var now = DateTime.UtcNow;
        var activeCount = await db.ShareGrants.CountAsync(
            g => g.HealthProfileId == request.HealthProfileId
                 && g.RevokedAt == null
                 && g.ExpiresAt > now, ct);

        if (activeCount >= MaxActiveGrantsPerProfile)
            throw new ConflictException(
                $"Hồ sơ đã có {MaxActiveGrantsPerProfile} liên kết chia sẻ đang hoạt động. Hãy thu hồi bớt trước khi tạo mới.");

        var token = ShareTokens.Generate();
        var scopes = request.Scope.Distinct().ToList();

        var grant = ShareGrant.Create(
            request.HealthProfileId,
            userId,
            ShareTokens.Hash(token),
            JsonSerializer.Serialize(scopes),
            request.TtlHours);

        db.ShareGrants.Add(grant);
        await db.SaveChangesAsync(ct);

        return new CreateShareGrantResultDto(grant.Id, token, scopes, grant.ExpiresAt);
    }
}
