using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Sharing.Common;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _email;

    public ForgotPasswordCommandHandler(IApplicationDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        if (user is null) return Result.Success(); // không tiết lộ email tồn tại hay không

        // BUG-11f: vô hiệu hóa mọi token reset cũ chưa dùng — tránh nhiều token hợp lệ song song trong 2h.
        var oldTokens = await _db.EmailVerifications
            .Where(v => v.UserId == user.Id && v.Type == VerificationType.PasswordReset && !v.IsUsed)
            .ToListAsync(ct);
        foreach (var old in oldTokens)
            old.MarkUsed();

        var token = Guid.NewGuid().ToString("N");
        var verification = EmailVerification.Create(user.Id, ShareTokens.Hash(token), VerificationType.PasswordReset, expiryHours: 2);
        _db.EmailVerifications.Add(verification);
        await _db.SaveChangesAsync(ct);

        await _email.SendPasswordResetAsync(user.Email, user.FirstName, token, ct);
        return Result.Success();
    }
}