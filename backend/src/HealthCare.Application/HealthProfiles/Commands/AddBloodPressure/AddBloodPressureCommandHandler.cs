using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.AddBloodPressure;

public class AddBloodPressureCommandHandler : IRequestHandler<AddBloodPressureCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPublisher _publisher;

    public AddBloodPressureCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPublisher publisher)
    {
        _context = context;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(AddBloodPressureCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var log = BloodPressureLog.Create(
            profile.Id,
            request.MeasuredAt,
            request.Systolic,
            request.Diastolic,
            request.Pulse,
            request.Arm,
            request.Position,
            request.Notes);

        _context.BloodPressureLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        foreach (var domainEvent in log.DomainEvents)
            await _publisher.Publish(domainEvent, ct);

        log.ClearDomainEvents();
        return log.Id;
    }
}