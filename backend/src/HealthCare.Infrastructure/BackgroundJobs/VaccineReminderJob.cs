using HealthCare.Domain.Entities.Notifications;
using HealthCare.Domain.Enums;
using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

public class VaccineReminderJob(ApplicationDbContext db, ILogger<VaccineReminderJob> logger)
{
    public async Task CreateUpcomingVaccineRemindersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var in7Days = today.AddDays(7);
        var in1Day = today.AddDays(1);

        var upcomingVaccines = await db.VaccineRecords
            .Where(v => v.NextDueDate.HasValue &&
                        v.NextDueDate >= today &&
                        v.NextDueDate <= in7Days)
            .Include(v => v.HealthProfile)
            .ToListAsync();

        foreach (var record in upcomingVaccines)
        {
            if (record.HealthProfile.UserId == null) continue;
            var userId = record.HealthProfile.UserId.Value;
            var daysUntil = record.NextDueDate!.Value.DayNumber - today.DayNumber;

            var alreadyExists = await db.Reminders.AnyAsync(r =>
                r.ReferenceId == record.Id &&
                r.UserId == userId &&
                r.Status == "pending");

            if (alreadyExists) continue;

            var remindAt = record.NextDueDate!.Value
                .ToDateTime(new TimeOnly(8, 0))
                .AddDays(-daysUntil == 1 ? 1 : 7);

            if (remindAt <= DateTime.UtcNow) continue;

            var suffix = daysUntil == 1 ? "ngày mai" : "7 ngày nữa";
            db.Reminders.Add(Reminder.Create(
                userId,
                record.HealthProfileId,
                ReminderType.Vaccine,
                $"Lịch tiêm {record.VaccineName} mũi {record.DoseNumber + 1} — {suffix}",
                remindAt,
                record.Id,
                $"Ngày tiêm dự kiến: {record.NextDueDate:dd/MM/yyyy}"));
        }

        await db.SaveChangesAsync();
        logger.LogInformation("VaccineReminderJob created reminders for {Count} upcoming vaccines", upcomingVaccines.Count);
    }
}
