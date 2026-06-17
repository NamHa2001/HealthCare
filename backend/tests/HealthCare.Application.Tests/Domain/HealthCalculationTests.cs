using FluentAssertions;
using HealthCare.Domain.Entities.HealthProfile;
using HealthCare.Domain.Services;

namespace HealthCare.Application.Tests.Domain;

/// <summary>SRS §14.1 Health Domain — 15 tests</summary>
public class HealthCalculationTests
{
    // ─── BMI label (ngưỡng châu Á) ────────────────────────────────────────────

    [Theory]
    [InlineData(16.0, "Thiếu cân")]
    [InlineData(18.4, "Thiếu cân")]
    [InlineData(18.5, "Bình thường")]
    [InlineData(22.9, "Bình thường")]
    [InlineData(23.0, "Thừa cân")]      // SRS: ngưỡng châu Á 23 = thừa cân
    [InlineData(24.9, "Thừa cân")]
    [InlineData(25.0, "Béo phì")]
    [InlineData(32.0, "Béo phì")]
    public void GetBmiLabel_ReturnsCorrectLabel(double bmi, string expected)
    {
        HealthCalculations.GetBmiLabel(bmi).Should().Be(expected);
    }

    // ─── BMI computed from weight/height ─────────────────────────────────────

    [Fact]
    public void ComputeBmi_ValidInputs_ReturnsExpectedValue()
    {
        // 70 kg / (1.70 m)^2 ≈ 24.22
        var bmi = HealthCalculations.ComputeBmi(70m, 170m);
        bmi.Should().NotBeNull();
        bmi!.Value.Should().BeApproximately(24.22m, 0.05m);
    }

    [Fact]
    public void ComputeBmi_NullWeight_ReturnsNull()
    {
        HealthCalculations.ComputeBmi(null, 170m).Should().BeNull();
    }

    [Fact]
    public void ComputeBmi_NullHeight_ReturnsNull()
    {
        HealthCalculations.ComputeBmi(70m, null).Should().BeNull();
    }

    [Fact]
    public void ComputeBmi_ZeroHeight_ReturnsNull()
    {
        HealthCalculations.ComputeBmi(70m, 0m).Should().BeNull();
    }

    // ─── BMI via HealthMeasurement.Create ────────────────────────────────────

    [Fact]
    public void HealthMeasurement_WithWeightAndHeight_ComputesBmi()
    {
        var m = HealthMeasurement.Create(Guid.NewGuid(), DateTime.UtcNow, weightKg: 60m, heightCm: 160m);
        // 60 / (1.60)^2 = 23.44 → Thừa cân
        m.Bmi.Should().NotBeNull();
        m.Bmi!.Value.Should().BeApproximately(23.44m, 0.05m);
    }

    [Fact]
    public void HealthMeasurement_WithoutHeight_BmiIsNull()
    {
        var m = HealthMeasurement.Create(Guid.NewGuid(), DateTime.UtcNow, weightKg: 70m);
        m.Bmi.Should().BeNull();
    }

    // ─── BP classification (5 levels) ────────────────────────────────────────

    [Theory]
    [InlineData(115, 75, "Bình thường")]
    [InlineData(119, 79, "Bình thường")]
    [InlineData(120, 75, "Tiền tăng huyết áp")]
    [InlineData(129, 79, "Tiền tăng huyết áp")]
    [InlineData(130, 80, "Cao độ 1")]
    [InlineData(139, 89, "Cao độ 1")]
    [InlineData(140, 90, "Cao độ 2")]
    [InlineData(160, 100, "Cao độ 2")]
    [InlineData(180, 120, "Khủng hoảng")]
    [InlineData(200, 130, "Khủng hoảng")]
    public void GetBpLabel_ReturnsCorrectLevel(int sys, int dia, string expected)
    {
        HealthCalculations.GetBpLabel(sys, dia).Should().Be(expected);
    }

    // ─── Measurement alert thresholds (SRS §3.2 — 9 loại chỉ số) ────────────

    [Theory]
    [InlineData(59, true)]   // dưới 60
    [InlineData(60, false)]
    [InlineData(100, false)]
    [InlineData(101, true)]  // trên 100
    public void IsHeartRateAlert_CorrectThreshold(int bpm, bool alert)
    {
        HealthCalculations.IsHeartRateAlert(bpm).Should().Be(alert);
    }

    [Fact]
    public void IsGlucoseAlert_Above7_IsTrue()
        => HealthCalculations.IsGlucoseAlert(7.1m).Should().BeTrue();

    [Fact]
    public void IsGlucoseAlert_AtOrBelow7_IsFalse()
        => HealthCalculations.IsGlucoseAlert(7.0m).Should().BeFalse();

    [Fact]
    public void IsSpo2Alert_Below95_IsTrue()
        => HealthCalculations.IsSpo2Alert(94.9m).Should().BeTrue();

    [Fact]
    public void IsTemperatureAlert_Above375_IsTrue()
        => HealthCalculations.IsTemperatureAlert(37.6m).Should().BeTrue();

    // ─── Weight change alert (>5% trong 1 tháng) ─────────────────────────────

    [Theory]
    [InlineData(70.0,  73.6,  true)]   // 5.14% > 5% → alert
    [InlineData(70.0,  73.5,  false)]  // 5.0% không vượt ngưỡng > 5%
    [InlineData(70.0,  66.0,  true)]   // 5.71% > 5% → alert
    [InlineData(100.0, 95.0,  false)]  // chính xác 5.0% — không vượt
    [InlineData(100.0, 94.9,  true)]   // 5.1% → alert
    public void IsWeightChangeAlert_CorrectThreshold(double baseline, double current, bool expected)
    {
        HealthCalculations.IsWeightChangeAlert(baseline, current).Should().Be(expected);
    }

    // ─── Health Score aggregate ───────────────────────────────────────────────

    [Fact]
    public void ComputeHealthScore_PerfectValues_Returns100()
    {
        // BMI 21 (bình thường=25) + BP 115/75 (bình thường=25) + vaccine 20/20 = 25 + compliance 1.0 = 25
        var score = HealthCalculations.ComputeHealthScore(
            bmi: 21.0,
            bp: (115, 75),
            completedVaccines: 20,
            totalVaccines: 20,
            medicationComplianceRate: 1.0);
        score.Should().Be(100);
    }

    [Fact]
    public void ComputeHealthScore_NoData_Returns48()
    {
        // All neutral = 12 + 12 + 12 + 12 = 48
        var score = HealthCalculations.ComputeHealthScore(null, null, 0, 0, null);
        score.Should().Be(48);
    }
}
