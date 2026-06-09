using HealthCare.Domain.Common;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities.HealthProfile;

public class HealthProfile : AuditableEntity
{
    public Guid? UserId { get; private set; }
    public Guid? FamilyMemberId { get; private set; }
    public BloodType? BloodType { get; private set; }
    public string? Allergies { get; private set; }           // JSON array
    public string? ChronicConditions { get; private set; }  // JSON array
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public byte[]? InsuranceNumber { get; private set; }    // AES-256 encrypted
    public string? PrimaryDoctor { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<HealthMeasurement> _measurements = [];
    private readonly List<BloodPressureLog> _bloodPressureLogs = [];
    private readonly List<HealthAlert> _alerts = [];

    public IReadOnlyCollection<HealthMeasurement> Measurements => _measurements.AsReadOnly();
    public IReadOnlyCollection<BloodPressureLog> BloodPressureLogs => _bloodPressureLogs.AsReadOnly();
    public IReadOnlyCollection<HealthAlert> Alerts => _alerts.AsReadOnly();

    private HealthProfile() { }

    public static HealthProfile CreateForUser(Guid userId) =>
        new() { UserId = userId };

    public static HealthProfile CreateForFamilyMember(Guid familyMemberId) =>
        new() { FamilyMemberId = familyMemberId };

    public void Update(
        BloodType? bloodType,
        string? allergies,
        string? chronicConditions,
        string? emergencyContactName,
        string? emergencyContactPhone,
        byte[]? insuranceNumber,
        string? primaryDoctor,
        string? notes)
    {
        BloodType = bloodType;
        Allergies = allergies;
        ChronicConditions = chronicConditions;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
        InsuranceNumber = insuranceNumber;
        PrimaryDoctor = primaryDoctor;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
