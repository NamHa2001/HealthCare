using HealthCare.Domain.Common;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Doctors;

public class DoctorProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string LicenseNumber { get; private set; } = null!; // số chứng chỉ hành nghề
    public string Specialty { get; private set; } = null!;
    public string Workplace { get; private set; } = null!;
    public string LicenseDocKeys { get; private set; } = null!; // JSON: storage keys ảnh CCHN
    public DoctorProfileStatus Status { get; private set; } = DoctorProfileStatus.Pending;
    public string? RejectReason { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public Guid? VerifiedBy { get; private set; }

    public User User { get; private set; } = null!;

    private DoctorProfile() { }

    public static DoctorProfile Create(Guid userId, string licenseNumber, string specialty, string workplace, string licenseDocKeysJson) =>
        new()
        {
            UserId = userId,
            LicenseNumber = licenseNumber.Trim(),
            Specialty = specialty.Trim(),
            Workplace = workplace.Trim(),
            LicenseDocKeys = licenseDocKeysJson,
        };

    public void Approve(Guid adminId)
    {
        Status = DoctorProfileStatus.Approved;
        RejectReason = null;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = adminId;
    }

    public void Reject(string reason, Guid adminId)
    {
        Status = DoctorProfileStatus.Rejected;
        RejectReason = reason;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = adminId;
    }

    public void Suspend(string reason, Guid adminId)
    {
        Status = DoctorProfileStatus.Suspended;
        RejectReason = reason;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = adminId;
    }

    /// <summary>Nộp lại hồ sơ sau khi bị từ chối — trở về trạng thái chờ duyệt.</summary>
    public void Resubmit(string licenseNumber, string specialty, string workplace, string? licenseDocKeysJson)
    {
        LicenseNumber = licenseNumber.Trim();
        Specialty = specialty.Trim();
        Workplace = workplace.Trim();
        if (licenseDocKeysJson is not null)
            LicenseDocKeys = licenseDocKeysJson;
        Status = DoctorProfileStatus.Pending;
        RejectReason = null;
        VerifiedAt = null;
        VerifiedBy = null;
    }
}
