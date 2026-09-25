using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Notifications.DTOs;
using HealthCare.Domain.Entities.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Notifications.Queries.GetNotificationPreference;

public class GetNotificationPreferenceQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetNotificationPreferenceQuery, NotificationPreferenceDto>
{
    public async Task<NotificationPreferenceDto> Handle(GetNotificationPreferenceQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var pref = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (pref is null)
        {
            pref = NotificationPreference.Create(userId);
            db.NotificationPreferences.Add(pref);
            await db.SaveChangesAsync(ct);
        }

        return MapToDto(pref);
    }

    private static NotificationPreferenceDto MapToDto(NotificationPreference p) => new(
        Timezone:           p.Timezone,
        QuietStartTime:     p.QuietStartTime.ToString("HH:mm"),
        QuietEndTime:       p.QuietEndTime.ToString("HH:mm"),
        PushEnabled:        p.PushEnabled,
        EmailEnabled:       p.EmailEnabled,
        VaccineReminder:    p.VaccineReminder,
        MedicationReminder: p.MedicationReminder,
        FollowupReminder:   p.FollowupReminder,
        HealthAlert:        p.HealthAlert,
        DoctorCriticalAlert: p.DoctorCriticalAlert,
        DoctorDailyDigest:   p.DoctorDailyDigest);
}
