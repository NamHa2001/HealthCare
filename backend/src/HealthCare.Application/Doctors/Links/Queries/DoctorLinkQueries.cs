using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Links.Queries;

// ─── Bệnh nhân: các liên kết/lời mời của mọi hồ sơ tôi quản lý ───────────────

public record GetMyDoctorLinksQuery : IRequest<IReadOnlyList<DoctorLinkDto>>;

public class GetMyDoctorLinksQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyDoctorLinksQuery, IReadOnlyList<DoctorLinkDto>>
{
    public async Task<IReadOnlyList<DoctorLinkDto>> Handle(GetMyDoctorLinksQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;

        // Hồ sơ cá nhân + hồ sơ family member do tôi quản lý
        var profileIds = await db.HealthProfiles
            .Where(p => p.UserId == userId
                || (p.FamilyMemberId != null && db.FamilyMembers.Any(
                    m => m.Id == p.FamilyMemberId && (m.ManagedBy == userId || m.UserId == userId))))
            .Select(p => p.Id)
            .ToListAsync(ct);

        var links = await db.PatientDoctorLinks
            .Where(l => profileIds.Contains(l.HealthProfileId))
            .OrderBy(l => l.Status == DoctorLinkStatus.Pending ? 0 : l.Status == DoctorLinkStatus.Active ? 1 : 2)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        var result = new List<DoctorLinkDto>();
        foreach (var l in links)
        {
            var doctor = await db.DoctorProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == l.DoctorUserId, ct);
            if (doctor is null) continue;

            var ownerName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, l.HealthProfileId, ct);
            result.Add(new DoctorLinkDto(
                l.Id, l.DoctorUserId, $"{doctor.User.FirstName} {doctor.User.LastName}",
                doctor.Specialty, doctor.Workplace,
                l.HealthProfileId, ownerName, l.InitiatedBy,
                l.Status.ToString().ToLowerInvariant(),
                l.ConsentAt, DoctorLinkHelper.GetScopes(l), l.ConsentText, l.CreatedAt));
        }
        return result;
    }
}

// ─── Bác sĩ: lời mời/liên kết đã gửi ─────────────────────────────────────────

public record GetDoctorInvitationsQuery(string? Status) : IRequest<IReadOnlyList<PatientLinkDto>>;

public class GetDoctorInvitationsQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetDoctorInvitationsQuery, IReadOnlyList<PatientLinkDto>>
{
    public async Task<IReadOnlyList<PatientLinkDto>> Handle(GetDoctorInvitationsQuery request, CancellationToken ct)
    {
        var query = db.PatientDoctorLinks
            .Where(l => l.DoctorUserId == currentUser.UserId);

        if (!string.IsNullOrEmpty(request.Status)
            && Enum.TryParse<DoctorLinkStatus>(request.Status, ignoreCase: true, out var status))
            query = query.Where(l => l.Status == status);

        var links = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        var result = new List<PatientLinkDto>();
        foreach (var l in links)
        {
            var name = await DoctorLinkHelper.ResolveOwnerNameAsync(db, l.HealthProfileId, ct);
            result.Add(new PatientLinkDto(
                l.Id, l.HealthProfileId, name, l.InitiatedBy,
                l.Status.ToString().ToLowerInvariant(),
                l.ConsentAt, DoctorLinkHelper.GetScopes(l), l.CreatedAt));
        }
        return result;
    }
}

// ─── Danh bạ bác sĩ đã xác minh ──────────────────────────────────────────────

public record SearchDoctorsQuery(string? Q) : IRequest<IReadOnlyList<DoctorSearchResultDto>>;

public class SearchDoctorsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<SearchDoctorsQuery, IReadOnlyList<DoctorSearchResultDto>>
{
    public async Task<IReadOnlyList<DoctorSearchResultDto>> Handle(SearchDoctorsQuery request, CancellationToken ct)
    {
        var query = db.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.Status == DoctorProfileStatus.Approved);

        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var q = request.Q.Trim();
            query = query.Where(d =>
                (d.User.FirstName + " " + d.User.LastName).Contains(q)
                || d.LicenseNumber.Contains(q)
                || d.Specialty.Contains(q)
                || d.Workplace.Contains(q));
        }

        var doctors = await query
            .OrderBy(d => d.User.FirstName)
            .Take(50)
            .ToListAsync(ct);

        return doctors.Select(d => new DoctorSearchResultDto(
            d.UserId, $"{d.User.FirstName} {d.User.LastName}",
            d.Specialty, d.Workplace, d.LicenseNumber))
            .ToList();
    }
}
