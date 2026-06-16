using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Commands.LogMedicationTaken;

public record LogMedicationTakenCommand(Guid LogId) : IRequest<MedicationLogDto>;
