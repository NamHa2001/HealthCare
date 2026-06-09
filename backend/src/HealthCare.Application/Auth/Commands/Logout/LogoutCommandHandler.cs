using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _db;
    public LogoutCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (token?.IsActive == true) token.Revoke();

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}