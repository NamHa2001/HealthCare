using HealthCare.Domain.Entities.Vaccines;
using HealthCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Persistence.Seeds;

public static class VaccineCatalogSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.VaccineCatalog.AnyAsync()) return;

        await SeedVaccine(db, "Viêm gan B", 3, "HepB", "[\"Viêm gan B\"]", true, 0,
            new[] { (1, (int?)0, (int?)1, (int?)null, (int?)0), (2, 1, 2, 28, 30), (3, 5, 7, 60, 180) });

        await SeedVaccine(db, "Bại liệt", 4, "OPV/IPV", "[\"Bại liệt\"]", true, 2,
            new[] { (1, (int?)2, (int?)3, (int?)null, (int?)null), (2, 3, 4, 28, 30), (3, 4, 5, 28, 30), (4, 18, 19, 180, 180) });

        await SeedVaccine(db, "Bạch hầu - Ho gà - Uốn ván", 5, "DTP", "[\"Bạch hầu\",\"Ho gà\",\"Uốn ván\"]", true, 2,
            new[] { (1, (int?)2, (int?)3, (int?)null, (int?)null), (2, 3, 4, 28, 30), (3, 4, 5, 28, 30), (4, 18, 19, 180, 180), (5, 48, 72, 365, 365) });

        await SeedVaccine(db, "Sởi - Quai bị - Rubella", 2, "MMR", "[\"Sởi\",\"Quai bị\",\"Rubella\"]", true, 9,
            new[] { (1, (int?)9, (int?)12, (int?)null, (int?)null), (2, 18, 19, 180, 180) });

        await SeedVaccine(db, "Thủy đậu", 2, "Varicella", "[\"Thủy đậu\"]", false, 12,
            new[] { (1, (int?)12, (int?)18, (int?)null, (int?)null), (2, 48, 72, 90, 90) });

        await SeedVaccine(db, "Cúm mùa", 1, "Flu", "[\"Cúm\"]", false, 6,
            new[] { (1, (int?)6, (int?)null, (int?)null, (int?)365) });

        await SeedVaccine(db, "HPV", 2, "HPV", "[\"Ung thư cổ tử cung\",\"Sùi mào gà\"]", false, 108,
            new[] { (1, (int?)108, (int?)312, (int?)null, (int?)null), (2, 108, 312, 150, 180) });

        await SeedVaccine(db, "COVID-19", 2, "COVID", "[\"COVID-19\"]", false, null,
            new[] { (1, (int?)null, (int?)null, (int?)null, (int?)null), (2, null, null, 21, 28) });
    }

    private static async Task SeedVaccine(
        ApplicationDbContext db,
        string name, int totalDoses, string shortName, string diseases,
        bool mandatory, int? ageStart,
        (int Dose, int? MinAge, int? MaxAge, int? MinDays, int? RecDays)[] rules)
    {
        var catalog = VaccineCatalog.Create(name, totalDoses, shortName, diseases, mandatory, ageStart);
        db.VaccineCatalog.Add(catalog);
        await db.SaveChangesAsync();

        foreach (var (dose, minAge, maxAge, minDays, recDays) in rules)
            db.VaccineScheduleRules.Add(VaccineScheduleRule.Create(catalog.Id, dose, minAge, maxAge, minDays, recDays));

        await db.SaveChangesAsync();
    }
}
