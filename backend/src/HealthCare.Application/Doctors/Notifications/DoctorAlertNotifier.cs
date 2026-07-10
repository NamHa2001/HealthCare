using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Application.Sharing.Common;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Doctors.Notifications;

public interface IDoctorAlertNotifier
{
    /// <summary>Thông báo cho các bác sĩ liên kết khi bệnh nhân có cảnh báo mới.
    /// Critical → email + push ngay lập tức; Warning → xếp hàng chờ daily digest.</summary>
    Task NotifyDoctorsAsync(IReadOnlyCollection<HealthAlert> alerts, CancellationToken ct);
}

public class DoctorAlertNotifier(
    IApplicationDbContext db,
    IEmailService email,
    INotificationService push,
    ILogger<DoctorAlertNotifier> logger) : IDoctorAlertNotifier
{
    public async Task NotifyDoctorsAsync(IReadOnlyCollection<HealthAlert> alerts, CancellationToken ct)
    {
        if (alerts.Count == 0) return;

        foreach (var group in alerts.GroupBy(a => a.HealthProfileId))
        {
            var links = await db.PatientDoctorLinks
                .Where(l => l.HealthProfileId == group.Key && l.Status == DoctorLinkStatus.Active)
                .ToListAsync(ct);
            if (links.Count == 0) continue;

            var patientName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, group.Key, ct);

            foreach (var link in links)
            {
                // Cảnh báo sinh từ measurements/BP — chỉ báo bác sĩ được chia sẻ các mục này
                var scopes = DoctorLinkHelper.GetScopes(link);
                if (!scopes.Contains(ShareScopes.Measurements) && !scopes.Contains(ShareScopes.BloodPressure))
                    continue;

                var doctor = await db.Users.FirstOrDefaultAsync(u => u.Id == link.DoctorUserId, ct);
                if (doctor is null) continue;

                foreach (var alert in group)
                {
                    if (alert.Severity == AlertSeverity.Critical)
                        await SendCriticalNowAsync(doctor, patientName, alert, ct);
                    else
                        await QueueForDigestAsync(link.DoctorUserId, alert, ct);
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SendCriticalNowAsync(
        Domain.Entities.Auth.User doctor, string patientName, HealthAlert alert, CancellationToken ct)
    {
        var already = await db.DoctorAlertDeliveries.AnyAsync(
            d => d.DoctorUserId == doctor.Id && d.HealthAlertId == alert.Id && d.Channel == "email", ct);
        if (already) return;

        var delivery = DoctorAlertDelivery.Create(doctor.Id, alert.Id, "email");
        db.DoctorAlertDeliveries.Add(delivery);

        try
        {
            await email.SendDoctorAlertAsync(doctor.Email, doctor.FirstName, patientName, alert.Message, ct);
            delivery.MarkSent();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email cảnh báo tới bác sĩ {Email}", doctor.Email);
            delivery.MarkFailed();
        }

        // Push tới các thiết bị đang active của bác sĩ (best-effort)
        var subs = await db.PushSubscriptions
            .Where(s => s.UserId == doctor.Id && s.IsActive)
            .ToListAsync(ct);
        if (subs.Count > 0)
        {
            var pushDelivery = DoctorAlertDelivery.Create(doctor.Id, alert.Id, "push");
            db.DoctorAlertDeliveries.Add(pushDelivery);
            var anySent = false;
            foreach (var sub in subs)
            {
                try
                {
                    await push.SendPushAsync(sub.FcmToken,
                        $"⚠ Bệnh nhân {patientName} — cảnh báo nghiêm trọng", alert.Message, ct);
                    anySent = true;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Push cảnh báo bác sĩ thất bại (sub {Id})", sub.Id);
                }
            }
            if (anySent) pushDelivery.MarkSent();
            else pushDelivery.MarkFailed();
        }
    }

    private async Task QueueForDigestAsync(Guid doctorUserId, HealthAlert alert, CancellationToken ct)
    {
        var already = await db.DoctorAlertDeliveries.AnyAsync(
            d => d.DoctorUserId == doctorUserId && d.HealthAlertId == alert.Id && d.Channel == "digest", ct);
        if (already) return;

        db.DoctorAlertDeliveries.Add(DoctorAlertDelivery.Create(doctorUserId, alert.Id, "digest"));
    }
}
