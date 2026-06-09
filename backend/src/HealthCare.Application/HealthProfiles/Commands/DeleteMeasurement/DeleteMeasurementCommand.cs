using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.DeleteMeasurement;

public record DeleteMeasurementCommand(Guid MeasurementId) : IRequest;