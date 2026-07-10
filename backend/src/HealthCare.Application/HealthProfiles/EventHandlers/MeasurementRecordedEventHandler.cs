using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Notifications;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Events;
using MediatR;

namespace HealthCare.Application.HealthProfiles.EventHandlers;

public class MeasurementRecordedEventHandler : INotificationHandler<MeasurementRecordedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IDoctorAlertNotifier _doctorNotifier;

    public MeasurementRecordedEventHandler(IApplicationDbContext context, IDoctorAlertNotifier doctorNotifier)
    {
        _context = context;
        _doctorNotifier = doctorNotifier;
    }

    public async Task Handle(MeasurementRecordedEvent notification, CancellationToken ct)
    {
        var alerts = new List<HealthAlert>();

        if (notification.Spo2Percent.HasValue && notification.Spo2Percent < 95m)
            alerts.Add(HealthAlert.Create(
                notification.HealthProfileId, AlertType.LowSpo2, AlertSeverity.Critical,
                $"SpO2 thấp: {notification.Spo2Percent}% (ngưỡng <95%)",
                measurementId: notification.MeasurementId));

        if (notification.HeartRateBpm.HasValue)
        {
            if (notification.HeartRateBpm < 60)
                alerts.Add(HealthAlert.Create(
                    notification.HealthProfileId, AlertType.LowHeartRate, AlertSeverity.Warning,
                    $"Nhịp tim thấp: {notification.HeartRateBpm} bpm (ngưỡng <60)",
                    measurementId: notification.MeasurementId));
            else if (notification.HeartRateBpm > 100)
                alerts.Add(HealthAlert.Create(
                    notification.HealthProfileId, AlertType.HighHeartRate, AlertSeverity.Warning,
                    $"Nhịp tim cao: {notification.HeartRateBpm} bpm (ngưỡng >100)",
                    measurementId: notification.MeasurementId));
        }

        if (notification.BloodGlucose.HasValue && notification.BloodGlucose > 7.0m)
            alerts.Add(HealthAlert.Create(
                notification.HealthProfileId, AlertType.HighGlucose, AlertSeverity.Warning,
                $"Đường huyết cao: {notification.BloodGlucose} mmol/L (ngưỡng >7.0 lúc đói)",
                measurementId: notification.MeasurementId));

        if (notification.Bmi.HasValue && notification.Bmi >= 23.0m)
        {
            var severity = notification.Bmi >= 25.0m ? AlertSeverity.Warning : AlertSeverity.Warning;
            var msg = notification.Bmi >= 25.0m
                ? $"BMI {notification.Bmi}: Béo phì (ngưỡng châu Á ≥25.0)"
                : $"BMI {notification.Bmi}: Thừa cân (ngưỡng châu Á ≥23.0)";
            alerts.Add(HealthAlert.Create(
                notification.HealthProfileId, AlertType.HighBMI, severity, msg,
                measurementId: notification.MeasurementId));
        }

        if (alerts.Count > 0)
        {
            _context.HealthAlerts.AddRange(alerts);
            await _context.SaveChangesAsync(ct);

            // Báo cho các bác sĩ đang theo dõi (DOCTOR_PORTAL.md §7)
            await _doctorNotifier.NotifyDoctorsAsync(alerts, ct);
        }
    }
}
