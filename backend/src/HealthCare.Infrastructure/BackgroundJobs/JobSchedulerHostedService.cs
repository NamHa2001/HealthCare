using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// Lịch chạy jobs nền (thay Hangfire ở giai đoạn hiện tại — SRS §11):
/// - ReminderProcessorJob:  mỗi 15 phút
/// - VaccineReminderJob:    hàng ngày 1:00 UTC
/// - DoctorDigestJob:       hàng ngày 0:00 UTC (≈ 7:00 sáng VN)
/// - ShareGrantCleanupJob:  hàng ngày 2:00 UTC — xóa ShareGrant hết hạn quá 30 ngày
/// - MedicationLogGeneratorJob: hàng ngày 3:00 UTC — sinh MedicationLog cho lịch uống thuốc hôm nay
///
/// Chạy nhiều instance API sẽ có nhiều tiến trình vào cùng vòng lặp này — mỗi job được bọc
/// bởi khóa phân tán (BackgroundJobLocks, xem JobLock.cs) để chỉ đúng 1 instance thực thi.
/// </summary>
public class JobSchedulerHostedService(
    IServiceProvider services,
    ILogger<JobSchedulerHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(10);
    private static readonly string InstanceId = $"{Environment.MachineName}:{Environment.ProcessId}";

    private DateTime _lastReminderRun = DateTime.MinValue;
    private DateOnly _lastVaccineRun = DateOnly.MinValue;
    private DateOnly _lastDigestRun = DateOnly.MinValue;
    private DateOnly _lastShareGrantCleanupRun = DateOnly.MinValue;
    private DateOnly _lastMedicationLogRun = DateOnly.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chờ app khởi động xong (migrations, seed) rồi mới chạy
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(nowUtc);

            // Cổng thời gian dùng biến in-memory riêng của từng instance — chỉ để tránh chạy dồn dập
            // (poll mỗi phút). Việc chỉ 1 instance thực sự thực thi do khóa phân tán trong RunAsync
            // quyết định, nên _lastXRun luôn được cập nhật dù instance này có thắng lock hay không
            // (thua nghĩa là instance khác đã/đang xử lý đúng lúc này).
            if (nowUtc - _lastReminderRun >= TimeSpan.FromMinutes(15))
            {
                await RunAsync<ReminderProcessorJob>("ReminderProcessorJob", j => j.ProcessPendingRemindersAsync());
                _lastReminderRun = nowUtc;
            }

            if (_lastDigestRun < today && nowUtc.Hour >= 0)
            {
                await RunAsync<DoctorDigestJob>("DoctorDigestJob", j => j.SendDailyDigestsAsync());
                _lastDigestRun = today;
            }

            if (_lastVaccineRun < today && nowUtc.Hour >= 1)
            {
                await RunAsync<VaccineReminderJob>("VaccineReminderJob", j => j.CreateUpcomingVaccineRemindersAsync());
                _lastVaccineRun = today;
            }

            if (_lastShareGrantCleanupRun < today && nowUtc.Hour >= 2)
            {
                await RunAsync<ShareGrantCleanupJob>("ShareGrantCleanupJob", j => j.DeleteExpiredGrantsAsync());
                _lastShareGrantCleanupRun = today;
            }

            if (_lastMedicationLogRun < today && nowUtc.Hour >= 3)
            {
                await RunAsync<MedicationLogGeneratorJob>("MedicationLogGeneratorJob", j => j.GenerateTodayLogsAsync());
                _lastMedicationLogRun = today;
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RunAsync<TJob>(string jobName, Func<TJob, Task> run) where TJob : notnull
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await TryClaimLockAsync(db, jobName))
            {
                logger.LogDebug("Bỏ qua job {Job} — instance khác đang giữ khóa", jobName);
                return;
            }

            var job = scope.ServiceProvider.GetRequiredService<TJob>();
            await run(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Background job {Job} thất bại", jobName);
        }
    }

    private static async Task<bool> TryClaimLockAsync(ApplicationDbContext db, string jobName)
    {
        // InMemory provider (dùng trong Integration.Tests) không hỗ trợ raw SQL — bỏ qua khóa,
        // không có nhiều instance để tranh chấp trong 1 tiến trình test.
        if (!db.Database.IsRelational())
            return true;

        var now = DateTime.UtcNow;
        var lockUntil = now.Add(LockDuration);

        // UPDATE có điều kiện — SQL Server khóa hàng trong lúc UPDATE nên hai instance chạy
        // đồng thời không thể cùng thắng: chỉ 1 câu lệnh thấy LockedUntil <= now và cập nhật được.
        var affected = await db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE BackgroundJobLocks
            SET LockedUntil = {lockUntil}, LockedBy = {InstanceId}
            WHERE JobName = {jobName} AND LockedUntil <= {now}");

        return affected > 0;
    }
}
