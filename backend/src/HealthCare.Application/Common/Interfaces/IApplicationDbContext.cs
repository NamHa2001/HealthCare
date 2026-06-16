using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Entities.MedicalHistory;
using HealthCare.Domain.Entities.Medications;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailVerification> EmailVerifications { get; }
    DbSet<HealthProfile> HealthProfiles { get; }
    DbSet<HealthMeasurement> HealthMeasurements { get; }
    DbSet<BloodPressureLog> BloodPressureLogs { get; }
    DbSet<HealthAlert> HealthAlerts { get; }
    DbSet<MedicalVisit> MedicalVisits { get; }
    DbSet<MedicalDocument> MedicalDocuments { get; }
    DbSet<DrugCatalog> DrugCatalog { get; }
    DbSet<Medication> Medications { get; }
    DbSet<MedicationSchedule> MedicationSchedules { get; }
    DbSet<MedicationLog> MedicationLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}