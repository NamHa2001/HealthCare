using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;

namespace HealthCare.Application.Admin.Commands.ResetUserPassword;

public class ResetUserPasswordCommandHandler(IApplicationDbContext db)
    : IRequestHandler<ResetUserPasswordCommand>
{
    public async Task Handle(ResetUserPasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([request.UserId], ct)
            ?? throw new NotFoundException("User", request.UserId);

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newHash);
        user.ResetFailedLogin();
        await db.SaveChangesAsync(ct);
    }
}
