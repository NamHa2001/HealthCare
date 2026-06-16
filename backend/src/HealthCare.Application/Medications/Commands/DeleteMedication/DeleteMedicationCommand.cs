using MediatR;

namespace HealthCare.Application.Medications.Commands.DeleteMedication;

public record DeleteMedicationCommand(Guid Id) : IRequest;
