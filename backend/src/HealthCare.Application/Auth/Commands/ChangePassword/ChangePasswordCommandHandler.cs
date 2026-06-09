using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Domain.Entities.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    public ChangePasswordCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([request.UserId], ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ForbiddenException("Mật khẩu hiện tại không đúng.");

        user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}