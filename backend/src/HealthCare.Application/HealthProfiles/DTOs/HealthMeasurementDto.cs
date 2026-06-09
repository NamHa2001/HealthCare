namespace HealthCare.Application.HealthProfiles.DTOs;

public class HealthMeasurementDto
{
    public Guid Id { get; set; }
    public Guid HealthProfileId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? Bmi { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? BodyTemperature { get; set; }
    public decimal? BloodGlucose { get; set; }
    public decimal? Spo2Percent { get; set; }
    public string? Notes { get; set; }
    public string Source { get; set; } = "manual";
    public DateTime CreatedAt { get; set; }
}