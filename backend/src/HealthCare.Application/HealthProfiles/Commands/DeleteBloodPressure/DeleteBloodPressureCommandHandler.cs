using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.DeleteBloodPressure;

public class DeleteBloodPressureCommandHandler : IRequestHandler<DeleteBloodPressureCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteBloodPressureCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteBloodPressureCommand request, CancellationToken ct)
    {
        var log = await _context.BloodPressureLogs
            .Include(b => b.HealthProfile)
            .FirstOrDefaultAsync(b => b.Id == request.BpLogId, ct)
            ?? throw new NotFoundException("BloodPressureLog", request.BpLogId);

        if (log.HealthProfile.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        _context.BloodPressureLogs.Remove(log);
        await _context.SaveChangesAsync(ct);
    }
}