using HealthCare.Domain.Entities.Medications;
using HealthCare.Domain.Entities.Notifications;
using HealthCare.Domain.Enums;
using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// BUG-14: không nơi nào từng tự sinh MedicationLog — "nhật ký uống thuốc" (GetMedicationSchedules,
/// LogMedicationTaken/Skipped) luôn thao tác trên bảng rỗng. Job này (theo tên đã định sẵn trong
/// PROJECT_STRUCTURE.md) chạy hàng ngày, sinh 1 MedicationLog "Pending" cho mỗi MedicationSchedule
/// đang active của ngày hôm nay, kèm Reminder nếu schedule bật nhắc nhở.
/// </summary>
public class MedicationLogGeneratorJob(ApplicationDbContext db, ILogger<MedicationLogGeneratorJob> logger)
{
    public async Task GenerateTodayLogsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayStart = today.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var activeSchedules = await db.MedicationSchedules
            .Include(s => s.Medication).ThenInclude(m => m.HealthProfile)
            .Where(s => s.Medication.DeletedAt == null &&
                        (s.Medication.IsOngoing ||
                         (s.Medication.StartDate <= today &&
                          (s.Medication.EndDate == null || s.Medication.EndDate >= today))))
            .ToListAsync();

        var created = 0;
        foreach (var schedule in activeSchedules)
        {
            var alreadyExists = await db.MedicationLogs.AnyAsync(l =>
                l.MedicationScheduleId == schedule.Id &&
                l.ScheduledAt >= dayStart && l.ScheduledAt < dayEnd);

            if (alreadyExists) continue;

            var scheduledAt = today.ToDateTime(schedule.ScheduledTime);
            var log = MedicationLog.Create(schedule.Id, scheduledAt);
            db.MedicationLogs.Add(log);
            created++;

            var profile = schedule.Medication.HealthProfile;
            if (schedule.ReminderEnabled && profile.UserId is not null)
            {
                var remindAt = scheduledAt.AddMinutes(-schedule.ReminderMinutesBefore);
                if (remindAt > DateTime.UtcNow)
                {
                    db.Reminders.Add(Reminder.Create(
                        profile.UserId.Value,
                        profile.Id,
                        ReminderType.Medication,
                        $"Đến giờ uống {schedule.Medication.DrugName}" +
                            (schedule.DosageAmount is null ? "" : $" ({schedule.DosageAmount})"),
                        remindAt,
                        log.Id));
                }
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("MedicationLogGeneratorJob created {Count} logs for {Date}", created, today);
    }
}
