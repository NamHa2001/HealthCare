using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.AcknowledgeAlert;

public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AcknowledgeAlertCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AcknowledgeAlertCommand request, CancellationToken ct)
    {
        var alert = await _context.HealthAlerts
            .Include(a => a.HealthProfile)
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, ct)
            ?? throw new NotFoundException("HealthAlert", request.AlertId);

        if (alert.HealthProfile.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        alert.Acknowledge();
        await _context.SaveChangesAsync(ct);
    }
}
