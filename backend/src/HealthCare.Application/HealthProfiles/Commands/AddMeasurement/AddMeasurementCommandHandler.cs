using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.HealthProfiles.Commands.AddMeasurement;

public class AddMeasurementCommandHandler : IRequestHandler<AddMeasurementCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPublisher _publisher;

    public AddMeasurementCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPublisher publisher)
    {
        _context = context;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(AddMeasurementCommand request, CancellationToken ct)
    {
        var profile = await _context.HealthProfiles
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, ct)
            ?? throw new NotFoundException("HealthProfile", _currentUser.UserId!);

        var measurement = HealthMeasurement.Create(
            profile.Id,
            request.MeasuredAt,
            request.WeightKg,
            request.HeightCm,
            request.HeartRateBpm,
            request.BodyTemperature,
            request.BloodGlucose,
            request.Spo2Percent,
            request.Notes,
            request.Source);

        _context.HealthMeasurements.Add(measurement);
        await _context.SaveChangesAsync(ct);

        foreach (var domainEvent in measurement.DomainEvents)
            await _publisher.Publish(domainEvent, ct);

        measurement.ClearDomainEvents();
        return measurement.Id;
    }
}