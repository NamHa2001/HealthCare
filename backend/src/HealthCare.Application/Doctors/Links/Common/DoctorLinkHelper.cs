using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Doctors;
using HealthCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Links.Common;

public static class DoctorLinkHelper
{
    public static List<string> GetScopes(PatientDoctorLink link) =>
        link.ConsentScope is null ? [] : JsonSerializer.Deserialize<List<string>>(link.ConsentScope) ?? [];

    /// <summary>Bác sĩ phải đang ở trạng thái Approved mới được thao tác liên kết.</summary>
    public static async Task<DoctorProfile> EnsureApprovedDoctorAsync(
        IApplicationDbContext db, Guid doctorUserId, CancellationToken ct)
    {
        var profile = await db.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == doctorUserId, ct);

        if (profile is null || profile.Status != DoctorProfileStatus.Approved)
            throw new ForbiddenException("Chỉ bác sĩ đã được xác minh mới thực hiện được thao tác này.");

        return profile;
    }

    /// <summary>Tạo mới hoặc kích hoạt lại liên kết (unique DoctorUserId + HealthProfileId).</summary>
    public static async Task<PatientDoctorLink> CreateOrReinviteAsync(
        IApplicationDbContext db, Guid doctorUserId, Guid healthProfileId, string initiatedBy, CancellationToken ct)
    {
        var existing = await db.PatientDoctorLinks.FirstOrDefaultAsync(
            l => l.DoctorUserId == doctorUserId && l.HealthProfileId == healthProfileId, ct);

        if (existing is null)
        {
            var link = PatientDoctorLink.Create(doctorUserId, healthProfileId, initiatedBy);
            db.PatientDoctorLinks.Add(link);
            return link;
        }

        switch (existing.Status)
        {
            case DoctorLinkStatus.Active:
                throw new ConflictException("Đã có liên kết đang hoạt động với bác sĩ này.");
            case DoctorLinkStatus.Pending:
                throw new ConflictException("Đã có lời mời đang chờ phản hồi.");
            default:
                existing.Reinvite(initiatedBy);
                return existing;
        }
    }

    /// <summary>Snapshot nguyên văn nội dung đồng ý — bất biến, phục vụ tuân thủ NĐ 13/2023.</summary>
    public static string BuildConsentText(DoctorProfile doctor, string ownerName, IReadOnlyList<string> scopes) =>
        $"Tôi đồng ý cho BS. {doctor.User.FirstName} {doctor.User.LastName} " +
        $"(CCHN {doctor.LicenseNumber}, {doctor.Workplace}) xem các dữ liệu sức khỏe sau " +
        $"của hồ sơ \"{ownerName}\": {string.Join(", ", scopes)}. " +
        $"Tôi hiểu rằng tôi có thể thu hồi quyền này bất cứ lúc nào. " +
        $"Thời điểm đồng ý: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.";

    /// <summary>Tên chủ hồ sơ: user cá nhân hoặc family member.</summary>
    public static async Task<string> ResolveOwnerNameAsync(
        IApplicationDbContext db, Guid healthProfileId, CancellationToken ct)
    {
        var profile = await db.HealthProfiles.FirstAsync(p => p.Id == healthProfileId, ct);

        if (profile.UserId is not null)
        {
            var user = await db.Users.FirstAsync(u => u.Id == profile.UserId, ct);
            return $"{user.FirstName} {user.LastName}";
        }

        var member = await db.FamilyMembers.FirstAsync(m => m.Id == profile.FamilyMemberId, ct);
        return member.FullName;
    }
}
