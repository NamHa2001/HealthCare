using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Admin.Commands.ToggleUserActive;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand>
{
    private readonly IApplicationDbContext _context;

    public ToggleUserActiveCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ToggleUserActiveCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (user.IsActive)
            user.Deactivate();
        else
            user.Activate();

        await _context.SaveChangesAsync(ct);
    }
}
