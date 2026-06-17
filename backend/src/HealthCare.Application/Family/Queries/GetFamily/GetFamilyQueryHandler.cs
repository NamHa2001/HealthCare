using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Family.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Family.Queries.GetFamily;

public class GetFamilyQueryHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<GetFamilyQuery, FamilyGroupDto?>
{
    public async Task<FamilyGroupDto?> Handle(GetFamilyQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var group = await db.FamilyGroups
            .Include(g => g.Members)
            .Where(g => g.AdminId == userId && !g.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (group is null) return null;

        var members = group.Members
            .Where(m => !m.IsDeleted)
            .Select(m => new FamilyMemberDto(
                m.Id,
                m.FamilyGroupId,
                m.UserId,
                m.FullName,
                m.DateOfBirth,
                m.Gender,
                m.Relationship,
                m.ManagedBy,
                m.CreatedAt))
            .ToList();

        return new FamilyGroupDto(group.Id, group.Name, group.AdminId, group.CreatedAt, members);
    }
}
