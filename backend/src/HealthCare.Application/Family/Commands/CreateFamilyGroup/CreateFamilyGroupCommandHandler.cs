using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Family.DTOs;
using HealthCare.Domain.Entities.Family;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Family.Commands.CreateFamilyGroup;

public class CreateFamilyGroupCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<CreateFamilyGroupCommand, FamilyGroupDto>
{
    public async Task<FamilyGroupDto> Handle(CreateFamilyGroupCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var existing = await db.FamilyGroups
            .AnyAsync(g => g.AdminId == userId && !g.IsDeleted, ct);

        if (existing)
            throw new ConflictException("Bạn đã có nhóm gia đình rồi.");

        var group = FamilyGroup.Create(request.Name, userId);
        db.FamilyGroups.Add(group);
        await db.SaveChangesAsync(ct);

        return new FamilyGroupDto(group.Id, group.Name, group.AdminId, group.CreatedAt, []);
    }
}
