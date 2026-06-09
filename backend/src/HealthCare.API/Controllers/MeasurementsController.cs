using HealthCare.Application.HealthProfiles.Commands.AddMeasurement;
using HealthCare.Application.HealthProfiles.Commands.DeleteMeasurement;
using HealthCare.Application.HealthProfiles.Queries.GetMeasurements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class MeasurementsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMeasurements(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetMeasurementsQuery(from, to, page, pageSize), ct));

    [HttpPost]
    public async Task<IActionResult> AddMeasurement(
        [FromBody] AddMeasurementCommand command, CancellationToken ct)
    {
        var id = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetMeasurements), new { }, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMeasurement(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteMeasurementCommand(id), ct);
        return NoContent();
    }
}
