using FluentAssertions;
using HealthCare.Application.Tests.Helpers;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Entities.Medications;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Tests.Domain;

/// <summary>SRS §14.1 Reminder Domain — 5 tests: compliance rate + medication score</summary>
public class MedicationComplianceTests
{
    // ─── HealthCalculations.MedicationScore ──────────────────────────────────

    [Fact]
    public void MedicationScore_FullCompliance_Returns25()
        => HealthCalculations.MedicationScore(1.0).Should().Be(25);

    [Fact]
    public void MedicationScore_ZeroCompliance_Returns0()
        => HealthCalculations.MedicationScore(0.0).Should().Be(0);

    [Fact]
    public void MedicationScore_NullCompliance_Returns12Neutral()
        => HealthCalculations.MedicationScore(null).Should().Be(12);

    [Fact]
    public void MedicationScore_HalfCompliance_Returns13()
        => HealthCalculations.MedicationScore(0.5).Should().Be(13);

    // ─── Compliance rate từ MedicationLog trong DB ───────────────────────────

    [Fact]
    public async Task MedicationLogs_ComplianceRate_CalculatesCorrectly()
    {
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var profile = HealthProfile.CreateForUser(userId);
        db.HealthProfiles.Add(profile);

        var drug = DrugCatalog.Create("Aspirin");
        db.DrugCatalog.Add(drug);
        await db.SaveChangesAsync();

        var medication = Medication.Create(profile.Id, "Aspirin 100mg", DateOnly.FromDateTime(DateTime.UtcNow),
            drugCatalogId: drug.Id);
        db.Medications.Add(medication);
        await db.SaveChangesAsync();

        var schedule = MedicationSchedule.Create(medication.Id, new TimeOnly(8, 0));
        db.MedicationSchedules.Add(schedule);
        await db.SaveChangesAsync();

        // 3 taken, 1 skipped → 75% compliance
        var log1 = MedicationLog.Create(schedule.Id, DateTime.UtcNow.AddDays(-3)); log1.MarkTaken();
        var log2 = MedicationLog.Create(schedule.Id, DateTime.UtcNow.AddDays(-2)); log2.MarkTaken();
        var log3 = MedicationLog.Create(schedule.Id, DateTime.UtcNow.AddDays(-1)); log3.MarkTaken();
        var log4 = MedicationLog.Create(schedule.Id, DateTime.UtcNow);             log4.MarkSkipped();

        db.MedicationLogs.AddRange(log1, log2, log3, log4);
        await db.SaveChangesAsync();

        var allLogs = db.MedicationLogs
            .Where(l => l.Status == MedicationLogStatus.Taken || l.Status == MedicationLogStatus.Skipped)
            .ToList();

        var taken = allLogs.Count(l => l.Status == MedicationLogStatus.Taken);
        var rate  = (double)taken / allLogs.Count;

        rate.Should().BeApproximately(0.75, 0.001);
        HealthCalculations.MedicationScore(rate).Should().Be(19); // round(0.75 * 25) = 19
    }
}
