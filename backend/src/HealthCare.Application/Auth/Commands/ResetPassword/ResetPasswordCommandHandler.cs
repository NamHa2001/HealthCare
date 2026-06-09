using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    public ResetPasswordCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var verification = await _db.EmailVerifications
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Token == request.Token, ct);

        if (verification is null || !verification.IsValid)
            throw new ForbiddenException("Token không hợp lệ hoặc đã hết hạn.");

        verification.User.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        verification.MarkUsed();
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}