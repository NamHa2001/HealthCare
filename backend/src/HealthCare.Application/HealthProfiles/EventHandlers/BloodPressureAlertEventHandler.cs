using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Notifications;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Events;
using MediatR;

namespace HealthCare.Application.HealthProfiles.EventHandlers;

public class BloodPressureAlertEventHandler : INotificationHandler<BloodPressureAlertEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IDoctorAlertNotifier _doctorNotifier;

    public BloodPressureAlertEventHandler(IApplicationDbContext context, IDoctorAlertNotifier doctorNotifier)
    {
        _context = context;
        _doctorNotifier = doctorNotifier;
    }

    public async Task Handle(BloodPressureAlertEvent notification, CancellationToken ct)
    {
        var severity = (notification.Systolic >= 180 || notification.Diastolic >= 120)
            ? AlertSeverity.Critical
            : AlertSeverity.Warning;

        var alert = HealthAlert.Create(
            notification.HealthProfileId,
            AlertType.HighBP,
            severity,
            $"Huyết áp cao: {notification.Systolic}/{notification.Diastolic} mmHg",
            bpLogId: notification.BpLogId);

        _context.HealthAlerts.Add(alert);
        await _context.SaveChangesAsync(ct);

        // Báo cho các bác sĩ đang theo dõi (DOCTOR_PORTAL.md §7)
        await _doctorNotifier.NotifyDoctorsAsync([alert], ct);
    }
}