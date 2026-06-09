using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Domain.Entities.Auth;
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

        var token = Guid.NewGuid().ToString("N");
        var verification = EmailVerification.Create(user.Id, token, expiryHours: 2);
        _db.EmailVerifications.Add(verification);
        await _db.SaveChangesAsync(ct);

        await _email.SendPasswordResetAsync(user.Email, user.FirstName, token, ct);
        return Result.Success();
    }
}