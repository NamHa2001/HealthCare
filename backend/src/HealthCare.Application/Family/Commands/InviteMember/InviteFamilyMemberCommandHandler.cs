using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Family;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Family.Commands.InviteMember;

public class InviteFamilyMemberCommandHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IEmailService email,
    ILogger<InviteFamilyMemberCommandHandler> logger)
    : IRequestHandler<InviteFamilyMemberCommand>
{
    public async Task Handle(InviteFamilyMemberCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var group = await db.FamilyGroups
            .FirstOrDefaultAsync(g => g.AdminId == userId && g.DeletedAt == null, ct)
            ?? throw new NotFoundException("FamilyGroup", userId);

        var invitedEmail = request.Email.ToLowerInvariant();

        // Prevent duplicate pending invites
        var hasPending = await db.FamilyInvitations
            .AnyAsync(i => i.FamilyGroupId == group.Id
                           && i.InvitedEmail == invitedEmail
                           && !i.IsAccepted
                           && i.ExpiresAt > DateTime.UtcNow, ct);

        if (hasPending)
            throw new ConflictException("Đã có lời mời đang chờ cho email này.");

        var token = Guid.NewGuid().ToString("N");
        var invitation = FamilyInvitation.Create(group.Id, invitedEmail, token);
        db.FamilyInvitations.Add(invitation);
        await db.SaveChangesAsync(ct);

        var inviter = await db.Users.FindAsync([userId], ct);
        var inviterName = inviter is null ? "Một thành viên" : $"{inviter.FirstName} {inviter.LastName}";

        try
        {
            await email.SendFamilyInviteAsync(invitedEmail, inviterName, group.Name, token, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not send family invite email to {Email}", invitedEmail);
        }
    }
}
