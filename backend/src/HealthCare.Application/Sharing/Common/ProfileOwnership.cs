using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Common;

public static class ProfileOwnership
{
    /// <summary>
    /// Hồ sơ thuộc quyền quản lý của user khi: là hồ sơ cá nhân của chính user,
    /// hoặc là hồ sơ family member do user quản lý (ManagedBy) / gắn với user.
    /// </summary>
    public static async Task<Domain.Entities.HealthProfile.HealthProfile> EnsureManagedByAsync(
        IApplicationDbContext db, Guid healthProfileId, Guid userId, CancellationToken ct)
    {
        var profile = await db.HealthProfiles
            .FirstOrDefaultAsync(p => p.Id == healthProfileId, ct)
            ?? throw new NotFoundException("HealthProfile", healthProfileId);

        if (profile.UserId == userId)
            return profile;

        if (profile.FamilyMemberId is not null)
        {
            var manages = await db.FamilyMembers.AnyAsync(
                m => m.Id == profile.FamilyMemberId && (m.ManagedBy == userId || m.UserId == userId), ct);
            if (manages)
                return profile;
        }

        throw new ForbiddenException("Bạn không có quyền với hồ sơ sức khỏe này.");
    }
}
