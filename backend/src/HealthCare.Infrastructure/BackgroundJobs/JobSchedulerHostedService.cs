using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// Lịch chạy jobs nền (thay Hangfire ở giai đoạn hiện tại — SRS §11):
/// - ReminderProcessorJob: mỗi 15 phút
/// - VaccineReminderJob:   hàng ngày 1:00 UTC
/// - DoctorDigestJob:      hàng ngày 0:00 UTC (≈ 7:00 sáng VN)
/// </summary>
public class JobSchedulerHostedService(
    IServiceProvider services,
    ILogger<JobSchedulerHostedService> logger) : BackgroundService
{
    private DateTime _lastReminderRun = DateTime.MinValue;
    private DateOnly _lastVaccineRun = DateOnly.MinValue;
    private DateOnly _lastDigestRun = DateOnly.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chờ app khởi động xong (migrations, seed) rồi mới chạy
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(nowUtc);

            if (nowUtc - _lastReminderRun >= TimeSpan.FromMinutes(15))
            {
                await RunAsync<ReminderProcessorJob>(j => j.ProcessPendingRemindersAsync());
                _lastReminderRun = nowUtc;
            }

            if (_lastDigestRun < today && nowUtc.Hour >= 0)
            {
                await RunAsync<DoctorDigestJob>(j => j.SendDailyDigestsAsync());
                _lastDigestRun = today;
            }

            if (_lastVaccineRun < today && nowUtc.Hour >= 1)
            {
                await RunAsync<VaccineReminderJob>(j => j.CreateUpcomingVaccineRemindersAsync());
                _lastVaccineRun = today;
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RunAsync<TJob>(Func<TJob, Task> run) where TJob : notnull
    {
        try
        {
            using var scope = services.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<TJob>();
            await run(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Background job {Job} thất bại", typeof(TJob).Name);
        }
    }
}
