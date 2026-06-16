using HealthCare.Application.Medications.DTOs;
using MediatR;

namespace HealthCare.Application.Medications.Commands.CreateMedication;

public record CreateMedicationCommand : IRequest<MedicationDto>
{
    public string DrugName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public bool IsOngoing { get; init; }
    public string? Strength { get; init; }
    public string? DosageForm { get; init; }
    public string? Instructions { get; init; }
    public DateOnly? EndDate { get; init; }
    public Guid? MedicalVisitId { get; init; }
    public Guid? DrugCatalogId { get; init; }
    public Guid? OcrSourceDocId { get; init; }
    public decimal? ConfidenceScore { get; init; }
}
