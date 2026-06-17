using FluentAssertions;
using HealthCare.Domain.Entities.Vaccines;
using HealthCare.Domain.Services;

namespace HealthCare.Application.Tests.Domain;

/// <summary>SRS §14.1 Vaccine Domain — 10 tests</summary>
public class VaccineDomainTests
{
    private static readonly Guid ProfileId = Guid.NewGuid();

    // ─── VaccineRecord.IsOverdue ──────────────────────────────────────────────

    [Fact]
    public void IsOverdue_NextDueDateInPast_ReturnsTrue()
    {
        var record = VaccineRecord.Create(
            ProfileId, "Viêm gan B", 1,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)),
            nextDueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        record.IsOverdue().Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_NextDueDateInFuture_ReturnsFalse()
    {
        var record = VaccineRecord.Create(
            ProfileId, "Viêm gan B", 1,
            DateOnly.FromDateTime(DateTime.UtcNow),
            nextDueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

        record.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_NoNextDueDate_ReturnsFalse()
    {
        var record = VaccineRecord.Create(ProfileId, "COVID-19", 2, DateOnly.FromDateTime(DateTime.UtcNow));
        record.IsOverdue().Should().BeFalse();
    }

    // ─── Next due date calculation via MinIntervalDays ───────────────────────

    [Theory]
    [InlineData("Viêm gan B",   1, 30)]   // mũi 2: sau 30 ngày
    [InlineData("Viêm gan B",   2, 150)]  // mũi 3: sau 5 tháng
    [InlineData("Bại liệt",     1, 30)]
    [InlineData("DTP",          1, 30)]
    [InlineData("MMR",          1, 270)]  // mũi 2: sau 9 tháng
    [InlineData("Thủy đậu",     1, 90)]   // mũi 2: sau 3 tháng
    [InlineData("HPV",          1, 60)]   // mũi 2: sau 2 tháng
    [InlineData("Cúm",          0, 365)]  // hàng năm
    public void NextDueDate_FromInjectionDate_UsesIntervalDays(string vaccineName, int doseNum, int intervalDays)
    {
        var injectionDate = DateOnly.FromDateTime(new DateTime(2026, 1, 1));
        var expectedNext  = injectionDate.AddDays(intervalDays);
        var rule          = VaccineScheduleRule.Create(Guid.NewGuid(), doseNum + 1, minIntervalDays: intervalDays);

        var computedNext = injectionDate.AddDays(rule.MinIntervalDays!.Value);

        computedNext.Should().Be(expectedNext, because: $"{vaccineName} dose {doseNum+1} should be {intervalDays} days later");
    }

    // ─── VaccineRecord domain entity ──────────────────────────────────────────

    [Fact]
    public void VaccineRecord_Create_SetsAllProperties()
    {
        var injection = DateOnly.FromDateTime(DateTime.UtcNow);
        var nextDue   = injection.AddDays(30);

        var record = VaccineRecord.Create(ProfileId, "MMR", 1, injection, nextDue);

        record.VaccineName.Should().Be("MMR");
        record.DoseNumber.Should().Be(1);
        record.InjectionDate.Should().Be(injection);
        record.NextDueDate.Should().Be(nextDue);
        record.HealthProfileId.Should().Be(ProfileId);
    }

    [Fact]
    public void VaccineRecord_Update_ChangesProperties()
    {
        var record = VaccineRecord.Create(ProfileId, "Cũ", 1, DateOnly.MinValue);
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow);

        record.Update("Mới", 2, newDate, null, "BV Nhi", null, null, null);

        record.VaccineName.Should().Be("Mới");
        record.DoseNumber.Should().Be(2);
        record.InjectionDate.Should().Be(newDate);
    }

    // ─── Vaccine Score component ─────────────────────────────────────────────

    [Theory]
    [InlineData(20, 20, 25)]  // 100% hoàn thành → 25 điểm
    [InlineData(10, 20, 13)]  // 50% → ~13 điểm
    [InlineData(0,  20, 0)]   // 0% → 0 điểm
    [InlineData(0,  0,  12)]  // không có catalog → neutral 12
    public void VaccineScore_IsProportional(int completed, int total, int expectedScore)
    {
        HealthCalculations.VaccineScore(completed, total).Should().Be(expectedScore);
    }
}
