using HealthCare.Domain.Enums;
using MediatR;

namespace HealthCare.Application.HealthProfiles.Commands.UpdateHealthProfile;

public record UpdateHealthProfileCommand : IRequest
{
    public BloodType? BloodType { get; init; }
    public string? Allergies { get; init; }
    public string? ChronicConditions { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? InsuranceNumberPlain { get; init; }
    public string? PrimaryDoctor { get; init; }
    public string? Notes { get; init; }
}