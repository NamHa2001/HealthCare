using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Commands.LogMedicationSkipped;

public record LogMedicationSkippedCommand(Guid LogId, string? SkipReason) : IRequest<MedicationLogDto>;
