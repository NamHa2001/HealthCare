using HealthCare.Application.Auth.DTOs;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Domain.Entities.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(IApplicationDbContext db, ICurrentUser currentUser)
    : IRequestHandler<UpdateProfileCommand, Result<UserInfoDto>>
{
    public async Task<Result<UserInfoDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("Người dùng chưa đăng nhập.");

        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new NotFoundException(nameof(User), userId);

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
        await db.SaveChangesAsync(ct);

        var roles = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .ToListAsync(ct);

        return Result<UserInfoDto>.Success(new UserInfoDto
        {
            Id              = user.Id,
            Email           = user.Email,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            PhoneNumber     = user.PhoneNumber,
            IsEmailVerified = user.IsEmailVerified,
            Roles           = roles,
        });
    }
}
