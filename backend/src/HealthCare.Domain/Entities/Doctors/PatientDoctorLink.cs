using HealthCare.Domain.Common;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.Doctors;

/// <summary>
/// Liên kết bác sĩ ↔ hồ sơ bệnh nhân. Consent record bất biến sau khi Accept —
/// chứng cứ tuân thủ Nghị định 13/2023/NĐ-CP (DOCTOR_PORTAL.md §5.2).
/// </summary>
public class PatientDoctorLink : BaseEntity
{
    public Guid DoctorUserId { get; private set; }
    public Guid HealthProfileId { get; private set; }
    public string InitiatedBy { get; private set; } = null!; // 'doctor' | 'patient'
    public DoctorLinkStatus Status { get; private set; } = DoctorLinkStatus.Pending;

    // Consent record — bất biến sau khi accept
    public DateTime? ConsentAt { get; private set; }
    public string? ConsentScope { get; private set; }     // JSON array
    public string? ConsentText { get; private set; }      // snapshot nguyên văn
    public string? ConsentIp { get; private set; }
    public string? ConsentUserAgent { get; private set; }
    public Guid? ConsentByUserId { get; private set; }

    public DateTime? RevokedAt { get; private set; }
    public string? RevokedBy { get; private set; }        // 'doctor' | 'patient' | 'system'

    public User DoctorUser { get; private set; } = null!;
    public HealthProfile.HealthProfile HealthProfile { get; private set; } = null!;

    private PatientDoctorLink() { }

    public static PatientDoctorLink Create(Guid doctorUserId, Guid healthProfileId, string initiatedBy) =>
        new()
        {
            DoctorUserId = doctorUserId,
            HealthProfileId = healthProfileId,
            InitiatedBy = initiatedBy,
        };

    /// <summary>Ghi consent record — do phía bệnh nhân thực hiện (khi accept lời mời của
    /// bác sĩ, hoặc ngay lúc chủ động mời bác sĩ).</summary>
    public void GiveConsent(string consentScopeJson, string consentText, string? ip, string? userAgent, Guid byUserId)
    {
        ConsentAt = DateTime.UtcNow;
        ConsentScope = consentScopeJson;
        ConsentText = consentText;
        ConsentIp = ip;
        ConsentUserAgent = userAgent;
        ConsentByUserId = byUserId;
    }

    public void Activate() => Status = DoctorLinkStatus.Active;

    /// <summary>Bệnh nhân chấp nhận lời mời của bác sĩ: consent + kích hoạt.</summary>
    public void Accept(string consentScopeJson, string consentText, string? ip, string? userAgent, Guid byUserId)
    {
        GiveConsent(consentScopeJson, consentText, ip, userAgent, byUserId);
        Activate();
    }

    public void Reject() => Status = DoctorLinkStatus.Rejected;

    public void Revoke(string by)
    {
        Status = DoctorLinkStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = by;
    }

    /// <summary>Mời lại sau khi bị từ chối/thu hồi — reset consent, quay về Pending.</summary>
    public void Reinvite(string initiatedBy)
    {
        InitiatedBy = initiatedBy;
        Status = DoctorLinkStatus.Pending;
        ConsentAt = null;
        ConsentScope = null;
        ConsentText = null;
        ConsentIp = null;
        ConsentUserAgent = null;
        ConsentByUserId = null;
        RevokedAt = null;
        RevokedBy = null;
    }
}
