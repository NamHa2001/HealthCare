using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.DeleteMeasurement;

public class DeleteMeasurementCommandHandler : IRequestHandler<DeleteMeasurementCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DeleteMeasurementCommandHandler(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteMeasurementCommand request, CancellationToken ct)
    {
        var measurement = await _context.HealthMeasurements
            .Include(m => m.HealthProfile)
            .FirstOrDefaultAsync(m => m.Id == request.MeasurementId, ct)
            ?? throw new NotFoundException("HealthMeasurement", request.MeasurementId);

        if (measurement.HealthProfile.UserId != _currentUser.UserId)
            throw new ForbiddenException();

        _context.HealthMeasurements.Remove(measurement);
        await _context.SaveChangesAsync(ct);
    }
}
