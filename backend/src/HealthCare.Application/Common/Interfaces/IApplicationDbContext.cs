using HealthCare.Domain.Entities.Audit;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Family;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Entities.MedicalHistory;
using HealthCare.Domain.Entities.Medications;
using HealthCare.Domain.Entities.Notifications;
using HealthCare.Domain.Entities.Vaccines;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FamilyGroup> FamilyGroups { get; }
    DbSet<FamilyMember> FamilyMembers { get; }
    DbSet<FamilyInvitation> FamilyInvitations { get; }
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
    DbSet<VaccineCatalog> VaccineCatalog { get; }
    DbSet<VaccineScheduleRule> VaccineScheduleRules { get; }
    DbSet<VaccineRecord> VaccineRecords { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<PushSubscription> PushSubscriptions { get; }
    DbSet<Reminder> Reminders { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}