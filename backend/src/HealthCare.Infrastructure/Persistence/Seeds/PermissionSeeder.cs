using HealthCare.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Seeds;

public static class PermissionSeeder
{
    private static readonly string[] Permissions =
    [
        "health_profile:read", "health_profile:write",
        "medical_history:read", "medical_history:write",
        "vaccine:read", "vaccine:write",
        "medication:read", "medication:write",
        "family:read", "family:write", "family:manage",
        "ocr:use",
        "admin:users", "admin:system",
    ];

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        foreach (var name in Permissions)
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == name))
                db.Permissions.Add(Permission.Create(name));
        }
        await db.SaveChangesAsync();
    }
}