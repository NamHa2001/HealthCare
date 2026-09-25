using HealthCare.Infrastructure.BackgroundJobs;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Seeds;

/// <summary>
/// Đảm bảo mỗi background job có sẵn 1 hàng lock trước khi JobSchedulerHostedService chạy,
/// để claim luôn là UPDATE (không cần INSERT lúc runtime → tránh race condition khi nhiều
/// instance cùng khởi động lần đầu).
/// </summary>
public static class JobLockSeeder
{
    // Phải khớp tên dùng trong JobSchedulerHostedService.
    public static readonly string[] KnownJobs =
    [
        "ReminderProcessorJob",
        "VaccineReminderJob",
        "DoctorDigestJob",
        "ShareGrantCleanupJob",
        "MedicationLogGeneratorJob",
    ];

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var available = DateTime.UtcNow.AddYears(-1);

        foreach (var jobName in KnownJobs)
        {
            if (!await db.JobLocks.AnyAsync(l => l.JobName == jobName))
                db.JobLocks.Add(new JobLock { JobName = jobName, LockedUntil = available });
        }

        await db.SaveChangesAsync();
    }
}
