using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Doctors.Commands.VerifyDoctor;

// Các command admin duyệt hồ sơ bác sĩ — DOCTOR_PORTAL.md §4.

public record ApproveDoctorCommand(Guid DoctorProfileId) : IRequest;
public record RejectDoctorCommand(Guid DoctorProfileId, string Reason) : IRequest;
public record SuspendDoctorCommand(Guid DoctorProfileId, string Reason) : IRequest;

public class ApproveDoctorCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IEmailService email,
    ILogger<ApproveDoctorCommandHandler> logger)
    : IRequestHandler<ApproveDoctorCommand>
{
    public async Task Handle(ApproveDoctorCommand request, CancellationToken ct)
    {
        var profile = await db.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorProfileId, ct)
            ?? throw new NotFoundException("DoctorProfile", request.DoctorProfileId);

        if (profile.Status == DoctorProfileStatus.Approved)
            throw new ConflictException("Hồ sơ đã được duyệt trước đó.");

        profile.Approve(currentUser.UserId!.Value);

        // Gán role 'doctor' — hiệu lực từ lần đăng nhập kế tiếp (JWT mới)
        var doctorRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "doctor", ct)
            ?? throw new NotFoundException("Role", "doctor");
        var hasRole = await db.UserRoles.AnyAsync(
            ur => ur.UserId == profile.UserId && ur.RoleId == doctorRole.Id, ct);
        if (!hasRole)
            db.UserRoles.Add(UserRole.Create(profile.UserId, doctorRole.Id));

        await db.SaveChangesAsync(ct);

        try
        {
            await email.SendDoctorApprovedAsync(profile.User.Email, profile.User.FirstName, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email duyệt bác sĩ tới {Email}", profile.User.Email);
        }
    }
}

public class RejectDoctorCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IEmailService email,
    ILogger<RejectDoctorCommandHandler> logger)
    : IRequestHandler<RejectDoctorCommand>
{
    public async Task Handle(RejectDoctorCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BadRequestException("Phải nêu lý do từ chối.");

        var profile = await db.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorProfileId, ct)
            ?? throw new NotFoundException("DoctorProfile", request.DoctorProfileId);

        profile.Reject(request.Reason, currentUser.UserId!.Value);
        await db.SaveChangesAsync(ct);

        try
        {
            await email.SendDoctorRejectedAsync(profile.User.Email, profile.User.FirstName, request.Reason, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email từ chối bác sĩ tới {Email}", profile.User.Email);
        }
    }
}

public class SuspendDoctorCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IEmailService email,
    ILogger<SuspendDoctorCommandHandler> logger)
    : IRequestHandler<SuspendDoctorCommand>
{
    public async Task Handle(SuspendDoctorCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BadRequestException("Phải nêu lý do tạm ngưng.");

        var profile = await db.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorProfileId, ct)
            ?? throw new NotFoundException("DoctorProfile", request.DoctorProfileId);

        if (profile.Status != DoctorProfileStatus.Approved)
            throw new ConflictException("Chỉ tạm ngưng được hồ sơ đang ở trạng thái đã duyệt.");

        profile.Suspend(request.Reason, currentUser.UserId!.Value);

        // Thu hồi mọi liên kết bệnh nhân đang hoạt động (DOCTOR_PORTAL.md §4.2)
        var activeLinks = await db.PatientDoctorLinks
            .Where(l => l.DoctorUserId == profile.UserId
                && (l.Status == DoctorLinkStatus.Active || l.Status == DoctorLinkStatus.Pending))
            .ToListAsync(ct);
        foreach (var link in activeLinks)
            link.Revoke("system");

        // Thu hồi role 'doctor' ngay lập tức
        var doctorRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "doctor", ct);
        if (doctorRole is not null)
        {
            var userRole = await db.UserRoles.FirstOrDefaultAsync(
                ur => ur.UserId == profile.UserId && ur.RoleId == doctorRole.Id, ct);
            if (userRole is not null)
                db.UserRoles.Remove(userRole);
        }

        await db.SaveChangesAsync(ct);

        try
        {
            await email.SendDoctorRejectedAsync(profile.User.Email, profile.User.FirstName,
                $"Tài khoản bác sĩ bị tạm ngưng: {request.Reason}", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email tạm ngưng bác sĩ tới {Email}", profile.User.Email);
        }
    }
}
