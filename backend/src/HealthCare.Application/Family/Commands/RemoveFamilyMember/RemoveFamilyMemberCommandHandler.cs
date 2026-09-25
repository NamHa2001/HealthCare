using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Family.Commands.RemoveFamilyMember;

public class RemoveFamilyMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<RemoveFamilyMemberCommand>
{
    public async Task Handle(RemoveFamilyMemberCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var member = await db.FamilyMembers
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.DeletedAt == null, ct)
            ?? throw new NotFoundException("FamilyMember", request.MemberId);

        // Kiểm tra quyền: chỉ family admin mới được xóa
        var group = await db.FamilyGroups
            .FirstOrDefaultAsync(g => g.Id == member.FamilyGroupId && g.DeletedAt == null, ct)
            ?? throw new NotFoundException("FamilyGroup", member.FamilyGroupId);

        if (group.AdminId != userId)
            throw new ForbiddenException("Bạn không có quyền xóa thành viên này.");

        member.SoftDelete();
        await db.SaveChangesAsync(ct);
    }
}
