using System.Text.Json;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Doctors.Links.Common;
using HealthCare.Application.Sharing.Common;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Doctors.Links.Commands;

// ─── Bác sĩ mời bệnh nhân (qua email) ────────────────────────────────────────

public record InvitePatientCommand(string PatientEmail) : IRequest<Guid>;

public class InvitePatientCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IEmailService email,
    ILogger<InvitePatientCommandHandler> logger)
    : IRequestHandler<InvitePatientCommand, Guid>
{
    public async Task<Guid> Handle(InvitePatientCommand request, CancellationToken ct)
    {
        var doctorUserId = currentUser.UserId!.Value;
        var doctor = await DoctorLinkHelper.EnsureApprovedDoctorAsync(db, doctorUserId, ct);

        var patientEmail = request.PatientEmail.Trim().ToLowerInvariant();
        var patient = await db.Users.FirstOrDefaultAsync(u => u.Email == patientEmail, ct)
            ?? throw new NotFoundException("Bệnh nhân", $"{patientEmail} — người này cần đăng ký Health+ trước");

        if (patient.Id == doctorUserId)
            throw new BadRequestException("Không thể tự mời chính mình.");

        var profile = await db.HealthProfiles.FirstOrDefaultAsync(p => p.UserId == patient.Id, ct)
            ?? throw new NotFoundException("HealthProfile", patient.Id);

        var link = await DoctorLinkHelper.CreateOrReinviteAsync(
            db, doctorUserId, profile.Id, "doctor", ct);

        await db.SaveChangesAsync(ct);

        try
        {
            var doctorName = $"{doctor.User.FirstName} {doctor.User.LastName}";
            await email.SendDoctorLinkInviteAsync(
                patient.Email, patient.FirstName,
                $"BS. {doctorName} ({doctor.Specialty}, {doctor.Workplace})", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email mời bệnh nhân {Email}", patient.Email);
        }

        return link.Id;
    }
}

// ─── Bệnh nhân mời bác sĩ (consent ghi ngay lúc mời, chờ bác sĩ chấp nhận) ───

public record InviteDoctorCommand(
    Guid DoctorUserId, Guid HealthProfileId, List<string> ConsentScope,
    string? IpAddress, string? UserAgent) : IRequest<Guid>;

public class InviteDoctorCommandHandler(
    IApplicationDbContext db, ICurrentUser currentUser, IEmailService email,
    ILogger<InviteDoctorCommandHandler> logger)
    : IRequestHandler<InviteDoctorCommand, Guid>
{
    public async Task<Guid> Handle(InviteDoctorCommand request, CancellationToken ct)
    {
        if (request.ConsentScope.Count == 0 || !request.ConsentScope.All(ShareScopes.IsValid))
            throw new BadRequestException("Phạm vi đồng ý không hợp lệ.");

        var userId = currentUser.UserId!.Value;
        await ProfileOwnership.EnsureManagedByAsync(db, request.HealthProfileId, userId, ct);
        var doctor = await DoctorLinkHelper.EnsureApprovedDoctorAsync(db, request.DoctorUserId, ct);

        if (request.DoctorUserId == userId)
            throw new BadRequestException("Không thể tự mời chính mình.");

        var link = await DoctorLinkHelper.CreateOrReinviteAsync(
            db, request.DoctorUserId, request.HealthProfileId, "patient", ct);

        // Người mời chính là chủ hồ sơ → consent record tạo ngay lúc mời
        var ownerName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, request.HealthProfileId, ct);
        var scopes = request.ConsentScope.Distinct().ToList();
        link.GiveConsent(
            JsonSerializer.Serialize(scopes),
            DoctorLinkHelper.BuildConsentText(doctor, ownerName, scopes),
            request.IpAddress, request.UserAgent, userId);

        await db.SaveChangesAsync(ct);

        try
        {
            var patientName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, request.HealthProfileId, ct);
            await email.SendDoctorLinkInviteAsync(
                doctor.User.Email, doctor.User.FirstName,
                $"Bệnh nhân {patientName}", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được email mời bác sĩ {Email}", doctor.User.Email);
        }

        return link.Id;
    }
}

// ─── Chấp nhận lời mời ───────────────────────────────────────────────────────
// Bác sĩ mời → bệnh nhân accept (cung cấp scope, tạo consent record).
// Bệnh nhân mời → bác sĩ accept (consent đã ghi lúc mời, chỉ kích hoạt).

public record AcceptDoctorLinkCommand(
    Guid LinkId, List<string> ConsentScope, string? IpAddress, string? UserAgent) : IRequest;

public class AcceptDoctorLinkCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<AcceptDoctorLinkCommand>
{
    public async Task Handle(AcceptDoctorLinkCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var link = await db.PatientDoctorLinks
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, ct)
            ?? throw new NotFoundException("PatientDoctorLink", request.LinkId);

        if (link.Status != DoctorLinkStatus.Pending)
            throw new ConflictException("Lời mời không còn ở trạng thái chờ phản hồi.");

        var doctor = await db.DoctorProfiles
            .Include(d => d.User)
            .FirstAsync(d => d.UserId == link.DoctorUserId, ct);
        if (doctor.Status != DoctorProfileStatus.Approved)
            throw new ConflictException("Bác sĩ này hiện không còn được xác minh.");

        if (link.InitiatedBy == "patient")
        {
            // Bác sĩ là người phản hồi — consent của bệnh nhân đã ghi lúc mời
            if (link.DoctorUserId != userId)
                throw new ForbiddenException("Chỉ bác sĩ được mời mới chấp nhận được lời mời này.");

            link.Activate();
        }
        else
        {
            // Bệnh nhân (chủ hồ sơ / người quản lý) phản hồi — cung cấp scope + consent
            if (request.ConsentScope.Count == 0 || !request.ConsentScope.All(ShareScopes.IsValid))
                throw new BadRequestException("Phạm vi đồng ý không hợp lệ.");

            await ProfileOwnership.EnsureManagedByAsync(db, link.HealthProfileId, userId, ct);

            var ownerName = await DoctorLinkHelper.ResolveOwnerNameAsync(db, link.HealthProfileId, ct);
            var scopes = request.ConsentScope.Distinct().ToList();

            link.Accept(
                JsonSerializer.Serialize(scopes),
                DoctorLinkHelper.BuildConsentText(doctor, ownerName, scopes),
                request.IpAddress, request.UserAgent, userId);
        }

        await db.SaveChangesAsync(ct);
    }
}

// ─── Từ chối / Thu hồi ───────────────────────────────────────────────────────

public record RejectDoctorLinkCommand(Guid LinkId) : IRequest;

public class RejectDoctorLinkCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<RejectDoctorLinkCommand>
{
    public async Task Handle(RejectDoctorLinkCommand request, CancellationToken ct)
    {
        var link = await db.PatientDoctorLinks
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, ct)
            ?? throw new NotFoundException("PatientDoctorLink", request.LinkId);

        if (link.Status != DoctorLinkStatus.Pending)
            throw new ConflictException("Lời mời không còn ở trạng thái chờ phản hồi.");

        // Người từ chối là phía được mời: bác sĩ (nếu bệnh nhân mời) hoặc chủ hồ sơ (nếu bác sĩ mời)
        if (link.InitiatedBy == "patient")
        {
            if (link.DoctorUserId != currentUser.UserId)
                throw new ForbiddenException("Chỉ bác sĩ được mời mới từ chối được lời mời này.");
        }
        else
        {
            await ProfileOwnership.EnsureManagedByAsync(db, link.HealthProfileId, currentUser.UserId!.Value, ct);
        }

        link.Reject();
        await db.SaveChangesAsync(ct);
    }
}

public record RevokeDoctorLinkCommand(Guid LinkId) : IRequest;

public class RevokeDoctorLinkCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<RevokeDoctorLinkCommand>
{
    public async Task Handle(RevokeDoctorLinkCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId!.Value;
        var link = await db.PatientDoctorLinks
            .FirstOrDefaultAsync(l => l.Id == request.LinkId, ct)
            ?? throw new NotFoundException("PatientDoctorLink", request.LinkId);

        // Hai phía đều thu hồi được: bác sĩ của link, hoặc người quản lý hồ sơ
        string revokedBy;
        if (link.DoctorUserId == userId)
        {
            revokedBy = "doctor";
        }
        else
        {
            await ProfileOwnership.EnsureManagedByAsync(db, link.HealthProfileId, userId, ct);
            revokedBy = "patient";
        }

        link.Revoke(revokedBy);
        await db.SaveChangesAsync(ct);
    }
}
