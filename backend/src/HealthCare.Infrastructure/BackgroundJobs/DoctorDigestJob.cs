using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// Daily digest cho bác sĩ (DOCTOR_PORTAL.md §7.2): gom các cảnh báo warning chưa gửi
/// thành 1 email/bác sĩ/ngày. Chạy 0:00 UTC ≈ 7:00 sáng Việt Nam.
/// </summary>
public class DoctorDigestJob(
    ApplicationDbContext db,
    IEmailService emailService,
    ILogger<DoctorDigestJob> logger)
{
    public async Task SendDailyDigestsAsync()
    {
        var pending = await db.DoctorAlertDeliveries
            .Where(d => d.Channel == "digest" && d.Status == "pending")
            .ToListAsync();

        if (pending.Count == 0) return;

        var alertIds = pending.Select(p => p.HealthAlertId).ToList();
        var alerts = await db.HealthAlerts
            .Where(a => alertIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        foreach (var group in pending.GroupBy(d => d.DoctorUserId))
        {
            var doctor = await db.Users.FirstOrDefaultAsync(u => u.Id == group.Key);
            if (doctor is null)
            {
                foreach (var d in group) d.MarkFailed();
                continue;
            }

            var lines = new List<string>();
            foreach (var delivery in group)
            {
                if (!alerts.TryGetValue(delivery.HealthAlertId, out var alert)) continue;
                var patientName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, alert.HealthProfileId, default);
                lines.Add($"<strong>{patientName}</strong>: {alert.Message} ({alert.CreatedAt:dd/MM HH:mm} UTC)");
            }

            if (lines.Count == 0) continue;

            try
            {
                await emailService.SendDoctorDigestAsync(doctor.Email, doctor.FirstName, lines);
                foreach (var d in group) d.MarkSent();
                logger.LogInformation("Digest gửi {Count} cảnh báo tới bác sĩ {Email}", lines.Count, doctor.Email);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Gửi digest thất bại cho bác sĩ {Email}", doctor.Email);
                foreach (var d in group) d.MarkFailed();
            }
        }

        await db.SaveChangesAsync();
    }
}
