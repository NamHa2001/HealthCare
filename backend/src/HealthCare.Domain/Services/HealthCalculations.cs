namespace HealthCare.Domain.Services;

/// <summary>
/// Pure static calculations — no DB, fully unit-testable.
/// </summary>
public static class HealthCalculations
{
    // ─── BMI ──────────────────────────────────────────────────────────────────

    public static decimal? ComputeBmi(decimal? weightKg, decimal? heightCm)
    {
        if (weightKg is null || heightCm is null || heightCm <= 0) return null;
        var heightM = heightCm.Value / 100m;
        return Math.Round(weightKg.Value / (heightM * heightM), 2);
    }

    public static string GetBmiLabel(double bmi) => bmi switch
    {
        < 18.5 => "Thiếu cân",
        < 23.0 => "Bình thường",
        < 25.0 => "Thừa cân",
        _      => "Béo phì"
    };

    // ─── Blood Pressure ───────────────────────────────────────────────────────

    /// <summary>5-level BP classification per JNC 8 / AHA 2017 adapted.</summary>
    public static string GetBpLabel(int systolic, int diastolic)
    {
        if (systolic >= 180 || diastolic >= 120) return "Khủng hoảng";
        if (systolic >= 140 || diastolic >= 90)  return "Cao độ 2";
        if (systolic >= 130 || diastolic >= 80)  return "Cao độ 1";
        if (systolic >= 120)                     return "Tiền tăng huyết áp";
        return "Bình thường";
    }

    public static bool IsBpAlert(int systolic, int diastolic)
        => systolic >= 140 || diastolic >= 90;

    // ─── Weight change ────────────────────────────────────────────────────────

    public static double WeightChangePercent(double baseline, double current)
    {
        if (baseline == 0) return 0;
        return Math.Abs(current - baseline) / baseline * 100.0;
    }

    public static bool IsWeightChangeAlert(double baseline, double current)
        => WeightChangePercent(baseline, current) > 5.0;

    // ─── Measurement thresholds (SRS §3.2) ───────────────────────────────────

    public static bool IsHeartRateAlert(int bpm)       => bpm < 60 || bpm > 100;
    public static bool IsGlucoseAlert(decimal mmol)    => mmol > 7.0m;
    public static bool IsSpo2Alert(decimal percent)    => percent < 95m;
    public static bool IsTemperatureAlert(decimal degC) => degC > 37.5m;
    public static bool IsSystolicAlert(int mmhg)       => mmhg >= 140;
    public static bool IsDiastolicAlert(int mmhg)      => mmhg >= 90;

    // ─── Health Score (SRS §8.2) ─────────────────────────────────────────────

    /// <summary>BMI score 0–25 pts.</summary>
    public static int BmiScore(double bmi) => GetBmiLabel(bmi) switch
    {
        "Bình thường" => 25,
        "Thiếu cân"   => 18,
        "Thừa cân"    => 13,
        _             => 5   // Béo phì
    };

    /// <summary>BP score 0–25 pts.</summary>
    public static int BpScore(int systolic, int diastolic) => GetBpLabel(systolic, diastolic) switch
    {
        "Bình thường"          => 25,
        "Tiền tăng huyết áp"   => 18,
        "Cao độ 1"             => 10,
        "Cao độ 2"             => 5,
        _                      => 0  // Khủng hoảng
    };

    /// <summary>Vaccine score 0–25 pts from completion ratio.</summary>
    public static int VaccineScore(int completed, int total)
    {
        if (total <= 0) return 12; // neutral when no catalog data
        var ratio = (double)completed / total;
        return (int)Math.Round(ratio * 25, MidpointRounding.AwayFromZero);
    }

    /// <summary>Medication compliance score 0–25 pts from rate 0.0–1.0.</summary>
    public static int MedicationScore(double? complianceRate)
    {
        if (complianceRate is null) return 12; // neutral when no log data
        return (int)Math.Round(complianceRate.Value * 25, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Aggregate health score 0–100.
    /// Components with no data use neutral mid-point so score stays meaningful.
    /// </summary>
    public static int ComputeHealthScore(
        double? bmi,
        (int Systolic, int Diastolic)? bp,
        int completedVaccines,
        int totalVaccines,
        double? medicationComplianceRate)
    {
        var bmiPts   = bmi.HasValue ? BmiScore(bmi.Value) : 12;
        var bpPts    = bp.HasValue  ? BpScore(bp.Value.Systolic, bp.Value.Diastolic) : 12;
        var vacPts   = VaccineScore(completedVaccines, totalVaccines);
        var medPts   = MedicationScore(medicationComplianceRate);
        return bmiPts + bpPts + vacPts + medPts;
    }
}
