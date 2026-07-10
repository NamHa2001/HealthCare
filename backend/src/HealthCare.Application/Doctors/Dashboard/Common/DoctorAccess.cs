using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Domain.Entities.Audit;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Dashboard.Common;

/// <summary>
/// Guard trung tâm cho mọi truy cập dữ liệu bệnh nhân của bác sĩ (DOCTOR_PORTAL.md §2.3):
/// 1) DoctorProfile Approved, 2) PatientDoctorLink Active, 3) đúng ConsentScope.
/// Mọi lượt truy cập được ghi AuditLog với TargetUserId = chủ hồ sơ.
/// </summary>
public static class DoctorAccess
{
    public static async Task<PatientDoctorLink> EnsureLinkedAsync(
        IApplicationDbContext db,
        Guid doctorUserId,
        Guid healthProfileId,
        string? requiredScope,
        string resource,
        CancellationToken ct)
    {
        var doctorApproved = await db.DoctorProfiles.AnyAsync(
            d => d.UserId == doctorUserId && d.Status == DoctorProfileStatus.Approved, ct);
        if (!doctorApproved)
            throw new ForbiddenException("Tài khoản bác sĩ chưa được xác minh hoặc đã bị tạm ngưng.");

        var link = await db.PatientDoctorLinks.FirstOrDefaultAsync(
            l => l.DoctorUserId == doctorUserId
                 && l.HealthProfileId == healthProfileId
                 && l.Status == DoctorLinkStatus.Active, ct)
            ?? throw new ForbiddenException("Không có liên kết đang hoạt động với bệnh nhân này.");

        if (requiredScope is not null && !DoctorLinkHelper.GetScopes(link).Contains(requiredScope))
            throw new ForbiddenException("Dữ liệu này nằm ngoài phạm vi bệnh nhân đã đồng ý.");

        await WriteAuditAsync(db, doctorUserId, healthProfileId, resource, "read", ct);
        return link;
    }

    public static async Task WriteAuditAsync(
        IApplicationDbContext db, Guid doctorUserId, Guid healthProfileId,
        string resource, string action, CancellationToken ct)
    {
        var profile = await db.HealthProfiles.FirstAsync(p => p.Id == healthProfileId, ct);
        Guid? targetUserId = profile.UserId;
        if (targetUserId is null && profile.FamilyMemberId is not null)
        {
            targetUserId = await db.FamilyMembers
                .Where(m => m.Id == profile.FamilyMemberId)
                .Select(m => m.ManagedBy)
                .FirstOrDefaultAsync(ct);
        }

        db.AuditLogs.Add(AuditLog.Create(
            eventType: "data_access",
            resource: resource,
            action: action,
            userId: doctorUserId,
            targetUserId: targetUserId,
            entityId: healthProfileId));

        await db.SaveChangesAsync(ct);
    }
}
