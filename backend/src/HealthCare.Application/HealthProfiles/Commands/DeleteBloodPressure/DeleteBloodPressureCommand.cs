using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.DeleteBloodPressure;

public record DeleteBloodPressureCommand(Guid BpLogId) : IRequest;