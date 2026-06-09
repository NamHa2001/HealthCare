using HealthCare.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using HealthCare.Domain.Entities.HealthProfile;

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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}