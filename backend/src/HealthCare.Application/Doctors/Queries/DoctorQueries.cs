using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Doctors.Queries;

// ─── User: hồ sơ bác sĩ của tôi ──────────────────────────────────────────────

public record GetMyDoctorProfileQuery : IRequest<DoctorProfileDto?>;

public class GetMyDoctorProfileQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetMyDoctorProfileQuery, DoctorProfileDto?>
{
    public async Task<DoctorProfileDto?> Handle(GetMyDoctorProfileQuery request, CancellationToken ct)
    {
        var p = await db.DoctorProfiles
            .FirstOrDefaultAsync(d => d.UserId == currentUser.UserId, ct);

        return p is null ? null : new DoctorProfileDto(
            p.Id, p.LicenseNumber, p.Specialty, p.Workplace,
            p.Status.ToString().ToLowerInvariant(), p.RejectReason, p.VerifiedAt, p.CreatedAt);
    }
}

// ─── Admin: danh sách hồ sơ chờ duyệt ────────────────────────────────────────

public record GetDoctorVerificationsQuery(string? Status) : IRequest<IReadOnlyList<DoctorVerificationListItemDto>>;

public class GetDoctorVerificationsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetDoctorVerificationsQuery, IReadOnlyList<DoctorVerificationListItemDto>>
{
    public async Task<IReadOnlyList<DoctorVerificationListItemDto>> Handle(
        GetDoctorVerificationsQuery request, CancellationToken ct)
    {
        var query = db.DoctorProfiles.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(request.Status)
            && Enum.TryParse<DoctorProfileStatus>(request.Status, ignoreCase: true, out var status))
            query = query.Where(d => d.Status == status);

        var items = await query
            .OrderBy(d => d.Status == DoctorProfileStatus.Pending ? 0 : 1)
            .ThenByDescending(d => d.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        return items.Select(d => new DoctorVerificationListItemDto(
            d.Id, d.UserId, $"{d.User.FirstName} {d.User.LastName}", d.User.Email,
            d.LicenseNumber, d.Specialty, d.Workplace,
            d.Status.ToString().ToLowerInvariant(), d.CreatedAt))
            .ToList();
    }
}

// ─── Admin: chi tiết + signed URLs ảnh CCHN ──────────────────────────────────

public record GetDoctorVerificationDetailQuery(Guid Id) : IRequest<DoctorVerificationDetailDto>;

public class GetDoctorVerificationDetailQueryHandler(IApplicationDbContext db, IFileStorageService storage)
    : IRequestHandler<GetDoctorVerificationDetailQuery, DoctorVerificationDetailDto>
{
    public async Task<DoctorVerificationDetailDto> Handle(
        GetDoctorVerificationDetailQuery request, CancellationToken ct)
    {
        var d = await db.DoctorProfiles
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new NotFoundException("DoctorProfile", request.Id);

        var keys = JsonSerializer.Deserialize<List<string>>(d.LicenseDocKeys) ?? [];
        var urls = new List<string>();
        foreach (var key in keys)
            urls.Add(await storage.GetSignedUrlAsync(key, expiryMinutes: 5, ct, StorageBucket.DoctorLicenses));

        return new DoctorVerificationDetailDto(
            d.Id, d.UserId, $"{d.User.FirstName} {d.User.LastName}", d.User.Email,
            d.User.PhoneNumber, d.LicenseNumber, d.Specialty, d.Workplace,
            d.Status.ToString().ToLowerInvariant(), d.RejectReason, d.VerifiedAt, d.CreatedAt, urls);
    }
}
