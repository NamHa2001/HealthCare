using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.AddMeasurement;

public record AddMeasurementCommand(
    DateTime MeasuredAt,
    decimal? WeightKg,
    decimal? HeightCm,
    int? HeartRateBpm,
    decimal? BodyTemperature,
    decimal? BloodGlucose,
    decimal? Spo2Percent,
    string? Notes,
    string Source = "manual"
) : IRequest<Guid>;