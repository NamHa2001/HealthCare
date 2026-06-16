using HealthCare.Application.MedicalHistory.DTOs;
using MediatR;

namespace HealthCare.Application.MedicalHistory.Commands.UpdateMedicalVisit;

public record UpdateMedicalVisitCommand : IRequest<MedicalVisitDto>
{
    public Guid Id { get; init; }
    public DateOnly VisitDate { get; init; }
    public string FacilityName { get; init; } = string.Empty;
    public string ChiefComplaint { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string? DoctorName { get; init; }
    public string? Icd10Code { get; init; }
    public string? Treatment { get; init; }
    public DateOnly? FollowUpDate { get; init; }
    public decimal? Cost { get; init; }
    public string? Notes { get; init; }
}
