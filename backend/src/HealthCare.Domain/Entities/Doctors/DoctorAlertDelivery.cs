using HealthCare.Domain.Common;

namespace HealthCare.Domain.Entities.Doctors;

/// <summary>
/// Theo dõi việc gửi cảnh báo sức khỏe của bệnh nhân tới bác sĩ liên kết (DOCTOR_PORTAL.md §7):
/// critical → gửi ngay (email/push), warning → gom vào daily digest.
/// Unique (DoctorUserId, HealthAlertId, Channel) chống gửi trùng.
/// </summary>
public class DoctorAlertDelivery : BaseEntity
{
    public Guid DoctorUserId { get; private set; }
    public Guid HealthAlertId { get; private set; }
    public string Channel { get; private set; } = null!; // 'email' | 'push' | 'digest'
    public string Status { get; private set; } = "pending"; // 'pending' | 'sent' | 'failed'
    public DateTime? SentAt { get; private set; }

    private DoctorAlertDelivery() { }

    public static DoctorAlertDelivery Create(Guid doctorUserId, Guid healthAlertId, string channel) =>
        new()
        {
            DoctorUserId = doctorUserId,
            HealthAlertId = healthAlertId,
            Channel = channel,
        };

    public void MarkSent()
    {
        Status = "sent";
        SentAt = DateTime.UtcNow;
    }

    public void MarkFailed() => Status = "failed";
}
