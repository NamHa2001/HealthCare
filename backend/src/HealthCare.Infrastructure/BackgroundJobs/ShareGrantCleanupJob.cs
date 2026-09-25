using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// DOCTOR_PORTAL.md §10: xóa ShareGrant hết hạn quá 30 ngày (kể cả đã bị revoke).
/// Chạy hàng ngày. Grant còn hạn hoặc mới hết hạn &lt;30 ngày được giữ lại để bệnh nhân
/// còn xem lại lịch sử chia sẻ ở tab "Đang chia sẻ".
/// </summary>
public class ShareGrantCleanupJob(ApplicationDbContext db, ILogger<ShareGrantCleanupJob> logger)
{
    public async Task DeleteExpiredGrantsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var stale = await db.ShareGrants
            .Where(g => g.ExpiresAt <= cutoff)
            .ToListAsync();

        if (stale.Count == 0) return;

        db.ShareGrants.RemoveRange(stale);
        await db.SaveChangesAsync();

        logger.LogInformation("ShareGrantCleanupJob deleted {Count} expired share grants", stale.Count);
    }
}
