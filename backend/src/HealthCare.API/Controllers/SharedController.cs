using HealthCare.Application.Sharing.Queries.GetSharedData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HealthCare.API.Controllers;

/// <summary>
/// Endpoints public cho link chia sẻ hồ sơ — người xem KHÔNG cần tài khoản.
/// Rate limit 30 req/phút/IP, mọi lỗi trả 404 đồng nhất (DOCTOR_PORTAL.md §3.3).
/// </summary>
[AllowAnonymous]
[EnableRateLimiting("shared")]
public class SharedController : BaseController
{
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("{token}")]
    public async Task<IActionResult> GetMeta(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedMetaQuery(token, ClientIp), ct));

    [HttpGet("{token}/profile")]
    public async Task<IActionResult> GetProfile(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedProfileQuery(token), ct));

    [HttpGet("{token}/measurements")]
    public async Task<IActionResult> GetMeasurements(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedMeasurementsQuery(token), ct));

    [HttpGet("{token}/blood-pressure")]
    public async Task<IActionResult> GetBloodPressure(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedBloodPressureQuery(token), ct));

    [HttpGet("{token}/medical-visits")]
    public async Task<IActionResult> GetVisits(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedVisitsQuery(token), ct));

    [HttpGet("{token}/medications")]
    public async Task<IActionResult> GetMedications(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedMedicationsQuery(token), ct));

    [HttpGet("{token}/vaccines")]
    public async Task<IActionResult> GetVaccines(string token, CancellationToken ct)
        => Ok(await Sender.Send(new GetSharedVaccinesQuery(token), ct));
}
