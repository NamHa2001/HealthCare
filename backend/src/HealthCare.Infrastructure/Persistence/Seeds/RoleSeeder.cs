using HealthCare.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Seeds;

public static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        var roles = new[]
        {
            Role.Create("admin", "Quản trị viên"),
            Role.Create("family_admin", "Quản lý gia đình"),
            Role.Create("user", "Người dùng"),
            Role.Create("readonly", "Chỉ xem"),
            Role.Create("doctor", "Bác sĩ đã xác minh"),
        };

        foreach (var role in roles)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == role.Name))
                db.Roles.Add(role);
        }

        await db.SaveChangesAsync();
    }
}