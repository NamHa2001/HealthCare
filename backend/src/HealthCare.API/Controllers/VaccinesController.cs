using HealthCare.Application.Vaccines.Commands.CreateVaccineRecord;
using HealthCare.Application.Vaccines.Commands.DeleteVaccineRecord;
using HealthCare.Application.Vaccines.Commands.UpdateVaccineRecord;
using HealthCare.Application.Vaccines.Queries.GetVaccineCatalog;
using HealthCare.Application.Vaccines.Queries.GetVaccinePassport;
using HealthCare.Application.Vaccines.Queries.GetVaccineProgress;
using HealthCare.Application.Vaccines.Queries.GetVaccineRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class VaccinesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetVaccineRecords(CancellationToken ct)
        => Ok(await Sender.Send(new GetVaccineRecordsQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> CreateVaccineRecord(
        [FromBody] CreateVaccineRecordCommand command,
        CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetVaccineRecords), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVaccineRecord(
        Guid id,
        [FromBody] UpdateVaccineRecordCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVaccineRecord(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteVaccineRecordCommand(id), ct);
        return NoContent();
    }

    [HttpGet("passport")]
    public async Task<IActionResult> DownloadPassport(CancellationToken ct)
    {
        var pdfBytes = await Sender.Send(new GetVaccinePassportQuery(), ct);
        var fileName = $"vaccine-passport-{DateTime.UtcNow:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog(CancellationToken ct)
        => Ok(await Sender.Send(new GetVaccineCatalogQuery(), ct));

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(CancellationToken ct)
        => Ok(await Sender.Send(new GetVaccineProgressQuery(), ct));
}
