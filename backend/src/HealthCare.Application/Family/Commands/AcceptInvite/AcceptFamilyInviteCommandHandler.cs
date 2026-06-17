using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Family;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Family.Commands.AcceptInvite;

public class AcceptFamilyInviteCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<AcceptFamilyInviteCommand>
{
    public async Task Handle(AcceptFamilyInviteCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var invitation = await db.FamilyInvitations
            .Include(i => i.FamilyGroup)
            .FirstOrDefaultAsync(i => i.Token == request.Token, ct)
            ?? throw new NotFoundException("FamilyInvitation", request.Token);

        if (invitation.IsExpired())
            throw new BadRequestException("Lời mời đã hết hạn.");

        if (invitation.IsAccepted)
            throw new ConflictException("Lời mời đã được chấp nhận trước đó.");

        var user = await db.Users.FindAsync([userId], ct)!;

        var member = FamilyMember.Create(
            invitation.FamilyGroupId,
            $"{user!.FirstName} {user.LastName}",
            DateOnly.FromDateTime(DateTime.UtcNow), // placeholder — user can update later
            "unknown",
            null,
            userId);

        db.FamilyMembers.Add(member);
        invitation.Accept();
        await db.SaveChangesAsync(ct);
    }
}
