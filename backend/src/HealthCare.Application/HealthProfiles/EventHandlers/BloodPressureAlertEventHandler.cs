using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Events;
using MediatR;

namespace HealthCare.Application.HealthProfiles.EventHandlers;

public class BloodPressureAlertEventHandler : INotificationHandler<BloodPressureAlertEvent>
{
    private readonly IApplicationDbContext _context;

    public BloodPressureAlertEventHandler(IApplicationDbContext context)
        => _context = context;

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
    }
}