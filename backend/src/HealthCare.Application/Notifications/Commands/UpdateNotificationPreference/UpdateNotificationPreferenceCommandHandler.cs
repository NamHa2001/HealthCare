using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Notifications.DTOs;
using HealthCare.Domain.Entities.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Notifications.Commands.UpdateNotificationPreference;

public class UpdateNotificationPreferenceCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateNotificationPreferenceCommand, NotificationPreferenceDto>
{
    public async Task<NotificationPreferenceDto> Handle(
        UpdateNotificationPreferenceCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var pref = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (pref is null)
        {
            pref = NotificationPreference.Create(userId);
            db.NotificationPreferences.Add(pref);
        }

        pref.Update(
            timezone:           request.Timezone,
            quietStartTime:     TimeOnly.ParseExact(request.QuietStartTime, "HH:mm"),
            quietEndTime:       TimeOnly.ParseExact(request.QuietEndTime,   "HH:mm"),
            pushEnabled:        request.PushEnabled,
            emailEnabled:       request.EmailEnabled,
            vaccineReminder:    request.VaccineReminder,
            medicationReminder: request.MedicationReminder,
            followupReminder:   request.FollowupReminder,
            healthAlert:        request.HealthAlert,
            doctorCriticalAlert: request.DoctorCriticalAlert,
            doctorDailyDigest:   request.DoctorDailyDigest);

        await db.SaveChangesAsync(ct);

        return new NotificationPreferenceDto(
            Timezone:           pref.Timezone,
            QuietStartTime:     pref.QuietStartTime.ToString("HH:mm"),
            QuietEndTime:       pref.QuietEndTime.ToString("HH:mm"),
            PushEnabled:        pref.PushEnabled,
            EmailEnabled:       pref.EmailEnabled,
            VaccineReminder:    pref.VaccineReminder,
            MedicationReminder: pref.MedicationReminder,
            FollowupReminder:   pref.FollowupReminder,
            HealthAlert:        pref.HealthAlert,
            DoctorCriticalAlert: pref.DoctorCriticalAlert,
            DoctorDailyDigest:   pref.DoctorDailyDigest);
    }
}
