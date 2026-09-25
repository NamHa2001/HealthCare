using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Family.DTOs;
using HealthCare.Domain.Entities.Family;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Family.Commands.AddFamilyMember;

public class AddFamilyMemberCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<AddFamilyMemberCommand, FamilyMemberDto>
{
    public async Task<FamilyMemberDto> Handle(AddFamilyMemberCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var group = await db.FamilyGroups
            .FirstOrDefaultAsync(g => g.AdminId == userId && g.DeletedAt == null, ct)
            ?? throw new NotFoundException("FamilyGroup", userId);

        var member = FamilyMember.Create(
            group.Id,
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.Relationship,
            userId);

        db.FamilyMembers.Add(member);

        // Tạo health profile cho member ngay khi thêm
        var profile = HealthProfile.CreateForFamilyMember(member.Id);
        db.HealthProfiles.Add(profile);

        await db.SaveChangesAsync(ct);

        return new FamilyMemberDto(
            member.Id,
            member.FamilyGroupId,
            member.UserId,
            member.FullName,
            member.DateOfBirth,
            member.Gender,
            member.Relationship,
            member.ManagedBy,
            member.CreatedAt);
    }
}
