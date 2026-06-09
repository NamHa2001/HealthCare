using HealthCare.Application.HealthProfiles.Commands.AddBloodPressure;
using HealthCare.Application.HealthProfiles.Commands.DeleteBloodPressure;
using HealthCare.Application.HealthProfiles.Queries.GetBloodPressureLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class BloodPressureController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetBloodPressureLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetBloodPressureLogsQuery(from, to, page, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> AddBloodPressure(
        [FromBody] AddBloodPressureCommand command, CancellationToken ct)
    {
        var id = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetBloodPressureLogs), new { }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBloodPressure(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteBloodPressureCommand(id), ct);
        return NoContent();
    }
}
