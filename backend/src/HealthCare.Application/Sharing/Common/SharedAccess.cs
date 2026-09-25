using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Sharing;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Sharing.Common;

/// <summary>
/// Validate token chia sẻ. Mọi trường hợp lỗi (không tồn tại / hết hạn / revoke / sai scope)
/// đều trả 404 đồng nhất để tránh dò token — theo DOCTOR_PORTAL.md §3.3.
/// </summary>
public static class SharedAccess
{
    public static async Task<ShareGrant> GetGrantAsync(
        IApplicationDbContext db, string token, string? requiredScope, string? ipAddress, CancellationToken ct)
    {
        var hash = ShareTokens.Hash(token);

        var grant = await db.ShareGrants
            .FirstOrDefaultAsync(g => g.TokenHash == hash, ct);

        if (grant is null || !grant.IsActive)
            throw new NotFoundException("Liên kết chia sẻ", "không hợp lệ hoặc đã hết hạn");

        if (requiredScope is not null && !GetScopes(grant).Contains(requiredScope))
            throw new NotFoundException("Liên kết chia sẻ", "không hợp lệ hoặc đã hết hạn");

        // BUG-07: ghi nhận MỌI lượt truy cập dữ liệu qua link chia sẻ, không chỉ endpoint meta.
        grant.RecordAccess(ipAddress);
        await db.SaveChangesAsync(ct);

        return grant;
    }

    public static List<string> GetScopes(ShareGrant grant) =>
        JsonSerializer.Deserialize<List<string>>(grant.Scope) ?? [];
}
