using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Commands.UpdateMedication;

public record UpdateMedicationCommand : IRequest<MedicationDto>
{
    public Guid Id { get; init; }
    public string DrugName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public bool IsOngoing { get; init; }
    public string? Strength { get; init; }
    public string? DosageForm { get; init; }
    public string? Instructions { get; init; }
    public DateOnly? EndDate { get; init; }
}
