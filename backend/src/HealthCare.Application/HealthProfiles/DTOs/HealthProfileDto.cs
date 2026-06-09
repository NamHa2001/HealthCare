using HealthCare.Domain.Enums;

namespace HealthCare.Application.HealthProfiles.DTOs;

public class HealthProfileDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? FamilyMemberId { get; set; }
    public BloodType? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? PrimaryDoctor { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
