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
                    var userLocalTime = TimeOnly.FromDateTime(DateTime.UtcNow);
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
}
