using HealthCare.Domain.Common;
using HealthCare.Domain.Events;

namespace HealthCare.Domain.Entities.HealthProfile;

public class HealthMeasurement : BaseEntity
{
    public Guid HealthProfileId { get; private set; }
    public DateTime MeasuredAt { get; private set; }
    public decimal? WeightKg { get; private set; }
    public decimal? HeightCm { get; private set; }
    public decimal? Bmi { get; private set; }           // tính tự động
    public int? HeartRateBpm { get; private set; }
    public decimal? BodyTemperature { get; private set; }
    public decimal? BloodGlucose { get; private set; }
    public decimal? Spo2Percent { get; private set; }
    public string? Notes { get; private set; }
    public string Source { get; private set; } = "manual";

    public HealthProfile HealthProfile { get; private set; } = null!;

    private HealthMeasurement() { }

    public static HealthMeasurement Create(
        Guid healthProfileId,
        DateTime measuredAt,
        decimal? weightKg = null,
        decimal? heightCm = null,
        int? heartRateBpm = null,
        decimal? bodyTemperature = null,
        decimal? bloodGlucose = null,
        decimal? spo2Percent = null,
        string? notes = null,
        string source = "manual")
    {
        var m = new HealthMeasurement
        {
            HealthProfileId = healthProfileId,
            MeasuredAt = measuredAt,
            WeightKg = weightKg,
            HeightCm = heightCm,
            HeartRateBpm = heartRateBpm,
            BodyTemperature = bodyTemperature,
            BloodGlucose = bloodGlucose,
            Spo2Percent = spo2Percent,
            Notes = notes,
            Source = source
        };

        // Tự tính BMI theo ngưỡng châu Á
        if (weightKg.HasValue && heightCm.HasValue && heightCm > 0)
        {
            var heightM = heightCm.Value / 100m;
            m.Bmi = Math.Round(weightKg.Value / (heightM * heightM), 2);
        }

        m.AddDomainEvent(new MeasurementRecordedEvent(healthProfileId, m.Id, m.Bmi, spo2Percent, heartRateBpm, bloodGlucose));
        return m;
    }
}