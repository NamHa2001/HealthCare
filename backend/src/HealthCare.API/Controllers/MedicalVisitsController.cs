using HealthCare.Application.MedicalHistory.Commands.CreateMedicalVisit;
using HealthCare.Application.MedicalHistory.Commands.DeleteMedicalVisit;
using HealthCare.Application.MedicalHistory.Commands.UpdateMedicalVisit;
using HealthCare.Application.MedicalHistory.Queries.GetMedicalVisitById;
using HealthCare.Application.MedicalHistory.Queries.GetMedicalVisits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class MedicalVisitsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetVisits(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default)
        => Ok(await Sender.Send(
            new GetMedicalVisitsQuery { Page = page, PageSize = pageSize, Year = year, Month = month }, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVisitById(Guid id, CancellationToken ct)
        => Ok(await Sender.Send(new GetMedicalVisitByIdQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> CreateVisit(
        [FromBody] CreateMedicalVisitCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetVisitById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVisit(
        Guid id, [FromBody] UpdateMedicalVisitCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command with { Id = id }, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVisit(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteMedicalVisitCommand(id), ct);
        return NoContent();
    }
}
