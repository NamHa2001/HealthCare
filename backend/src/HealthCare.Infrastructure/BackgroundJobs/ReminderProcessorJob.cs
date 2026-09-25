using HealthCare.Application.Common.Interfaces;
using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

public class ReminderProcessorJob(
    ApplicationDbContext db,
    INotificationService notificationService,
    IEmailService emailService,
    ILogger<ReminderProcessorJob> logger)
{
    public async Task ProcessPendingRemindersAsync()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(15);
        var reminders = await db.Reminders
            .Where(r => r.Status == "pending" && r.RemindAt <= cutoff)
            .ToListAsync();

        foreach (var reminder in reminders)
        {
            try
            {
                var prefs = await db.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == reminder.UserId);

                if (prefs != null)
                {
                    // BUG-16: trước đây gán thẳng từ UtcNow, không quy đổi theo prefs.Timezone —
                    // quiet hours lệch 7 tiếng so với giờ Việt Nam.
                    var userLocalTime = TimeOnly.FromDateTime(ToUserLocalTime(DateTime.UtcNow, prefs.Timezone));
                    if (prefs.IsInQuietHours(userLocalTime))
                    {
                        logger.LogInformation("Skipping reminder {Id} — quiet hours", reminder.Id);
                        continue;
                    }
                }

                var pushSent = false;
                var subscriptions = await db.PushSubscriptions
                    .Where(s => s.UserId == reminder.UserId && s.IsActive)
                    .ToListAsync();

                foreach (var sub in subscriptions)
                {
                    try
                    {
                        await notificationService.SendPushAsync(sub.FcmToken, reminder.Title, reminder.Body ?? string.Empty);
                        sub.RecordUsage();
                        pushSent = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Push failed for subscription {Id}", sub.Id);
                    }
                }

                if (!pushSent)
                {
                    var user = await db.Users.FindAsync(reminder.UserId);
                    if (user != null)
                    {
                        try
                        {
                            await emailService.SendReminderAsync(user.Email, reminder.Title, reminder.Body ?? string.Empty);
                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Email fallback failed for reminder {Id}", reminder.Id);
                            reminder.MarkFailed();
                            continue;
                        }
                    }
                }

                reminder.MarkSent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process reminder {Id}", reminder.Id);
                reminder.MarkFailed();
            }
        }

        await db.SaveChangesAsync();
    }

    private static DateTime ToUserLocalTime(DateTime utcNow, string timezone)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, tz);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Fallback: Asia/Ho_Chi_Minh = UTC+7 cố định (không có DST) — an toàn nếu hệ điều hành
            // thiếu dữ liệu IANA timezone.
            return utcNow.AddHours(7);
        }
    }
}
