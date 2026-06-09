namespace HealthCare.Application.HealthProfiles.DTOs;

public class BloodPressureLogDto
{
    public Guid Id { get; set; }
    public Guid HealthProfileId { get; set; }
    public DateTime MeasuredAt { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public int? Pulse { get; set; }
    public string Arm { get; set; } = "left";
    public string Position { get; set; } = "sitting";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
