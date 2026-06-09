using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.AddBloodPressure;

public record AddBloodPressureCommand(
    DateTime MeasuredAt,
    int Systolic,
    int Diastolic,
    int? Pulse,
    string Arm = "left",
    string Position = "sitting",
    string? Notes = null
) : IRequest<Guid>;