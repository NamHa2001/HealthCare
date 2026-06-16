namespace HealthCare.Application.Medications.DTOs;

public class ComplianceReportDto
{
    public string WeekLabel { get; init; } = string.Empty;   // e.g. "10/06 – 16/06"
    public int TotalScheduled { get; init; }
    public int TotalTaken { get; init; }
    public decimal CompliancePercent { get; init; }
}
