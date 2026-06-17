using FluentAssertions;
using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.HealthProfiles.Commands.AddMeasurement;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.HealthProfile;
using MediatR;
using Moq;

namespace HealthCare.Application.Tests.HealthProfiles;

public class AddMeasurementCommandHandlerTests
{
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();

    private AddMeasurementCommandHandler CreateHandler(HealthCare.Infrastructure.Persistence.ApplicationDbContext db)
        => new(db, _currentUserMock.Object, _publisherMock.Object);

    [Fact]
    public async Task Handle_ProfileExists_CreatesMeasurementAndReturnsId()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var profile = HealthProfile.CreateForUser(userId);
        db.HealthProfiles.Add(profile);
        await db.SaveChangesAsync();

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        var handler = CreateHandler(db);
        var cmd = new AddMeasurementCommand(
            MeasuredAt: DateTime.UtcNow,
            WeightKg: 70m,
            HeightCm: 170m,
            HeartRateBpm: 75,
            BodyTemperature: null,
            BloodGlucose: null,
            Spo2Percent: null,
            Notes: null,
            Source: "manual"
        );

        var id = await handler.Handle(cmd, CancellationToken.None);

        id.Should().NotBe(Guid.Empty);
        db.HealthMeasurements.Should().HaveCount(1);
        db.HealthMeasurements.First().WeightKg.Should().Be(70m);
    }

    [Fact]
    public async Task Handle_NoProfile_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        var handler = CreateHandler(db);
        var cmd = new AddMeasurementCommand(DateTime.UtcNow, null, null, null, null, null, null, null, "manual");

        var act = () => handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithWeightAndHeight_ComputesBmi()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        db.HealthProfiles.Add(HealthProfile.CreateForUser(userId));
        await db.SaveChangesAsync();

        _currentUserMock.Setup(u => u.UserId).Returns(userId);

        var handler = CreateHandler(db);
        // BMI = 70 / (1.70^2) ≈ 24.22
        var cmd = new AddMeasurementCommand(DateTime.UtcNow, 70m, 170m, null, null, null, null, null, "manual");

        var id = await handler.Handle(cmd, CancellationToken.None);

        var saved = db.HealthMeasurements.Find(id);
        saved!.Bmi.Should().NotBeNull();
        saved.Bmi!.Value.Should().BeApproximately(24.22m, 0.1m);
    }
}
