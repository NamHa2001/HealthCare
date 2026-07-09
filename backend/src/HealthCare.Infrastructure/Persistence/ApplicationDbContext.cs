using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Common;
using HealthCare.Domain.Entities.Audit;
using HealthCare.Domain.Entities.Auth;
using HealthCare.Domain.Entities.Family;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Entities.MedicalHistory;
using HealthCare.Domain.Entities.Medications;
using HealthCare.Domain.Entities.Notifications;
using HealthCare.Domain.Entities.Sharing;
using HealthCare.Domain.Entities.Vaccines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthCare.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<FamilyGroup> FamilyGroups => Set<FamilyGroup>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<FamilyInvitation> FamilyInvitations => Set<FamilyInvitation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<HealthProfile> HealthProfiles => Set<HealthProfile>();
    public DbSet<HealthMeasurement> HealthMeasurements => Set<HealthMeasurement>();
    public DbSet<BloodPressureLog> BloodPressureLogs => Set<BloodPressureLog>();
    public DbSet<HealthAlert> HealthAlerts => Set<HealthAlert>();
    public DbSet<MedicalVisit> MedicalVisits => Set<MedicalVisit>();
    public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();
    public DbSet<DrugCatalog> DrugCatalog => Set<DrugCatalog>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
    public DbSet<MedicationLog> MedicationLogs => Set<MedicationLog>();
    public DbSet<VaccineCatalog> VaccineCatalog => Set<VaccineCatalog>();
    public DbSet<VaccineScheduleRule> VaccineScheduleRules => Set<VaccineScheduleRule>();
    public DbSet<VaccineRecord> VaccineRecords => Set<VaccineRecord>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ShareGrant> ShareGrants => Set<ShareGrant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // SQL Server lưu DateTime không có timezone info. EF Core đọc lại trả Kind=Unspecified
        // → System.Text.Json serialize không có "Z" → Angular DatePipe hiểu là local time → sai 7h (UTC+7).
        // Fix: global converter đảm bảo mọi DateTime đọc từ DB đều là Kind=Utc.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utcConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(utcNullableConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}