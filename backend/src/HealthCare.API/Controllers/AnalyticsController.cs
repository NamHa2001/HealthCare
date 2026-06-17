using HealthCare.Application.Analytics.Queries.GetBmiTrend;
using HealthCare.Application.Analytics.Queries.GetBpTrend;
using HealthCare.Application.Analytics.Queries.GetGlucoseTrend;
using HealthCare.Application.Analytics.Queries.GetHealthScore;
using HealthCare.Application.Analytics.Queries.GetHealthSummary;
using HealthCare.Application.Analytics.Queries.GetMedicationCompliance;
using HealthCare.Application.Analytics.Queries.GetVaccineProgress;
using HealthCare.Application.Analytics.Queries.GetVisitFrequency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class AnalyticsController : BaseController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(await Sender.Send(new GetHealthSummaryQuery(), ct));

    [HttpGet("health-score")]
    public async Task<IActionResult> GetHealthScore(CancellationToken ct)
        => Ok(await Sender.Send(new GetHealthScoreQuery(), ct));

    [HttpGet("bmi-trend")]
    public async Task<IActionResult> GetBmiTrend([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetBmiTrendQuery(days), ct));

    [HttpGet("bp-trend")]
    public async Task<IActionResult> GetBpTrend([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetBpTrendQuery(days), ct));

    [HttpGet("vaccine-progress")]
    public async Task<IActionResult> GetVaccineProgress(CancellationToken ct)
        => Ok(await Sender.Send(new GetVaccineProgressQuery(), ct));

    [HttpGet("medication-compliance")]
    public async Task<IActionResult> GetMedicationCompliance([FromQuery] int weeks = 8, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetMedicationComplianceQuery(weeks), ct));

    [HttpGet("glucose-trend")]
    public async Task<IActionResult> GetGlucoseTrend([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetGlucoseTrendQuery(days), ct));

    [HttpGet("visit-frequency")]
    public async Task<IActionResult> GetVisitFrequency([FromQuery] int months = 6, CancellationToken ct = default)
        => Ok(await Sender.Send(new GetVisitFrequencyQuery(months), ct));
}
