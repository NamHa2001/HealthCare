using HealthCare.Application.Medications.Commands.AddMedicationSchedule;
using HealthCare.Application.Medications.Commands.CreateMedication;
using HealthCare.Application.Medications.Commands.DeleteMedication;
using HealthCare.Application.Medications.Commands.DeleteMedicationSchedule;
using HealthCare.Application.Medications.Commands.LogMedicationSkipped;
using HealthCare.Application.Medications.Commands.LogMedicationTaken;
using HealthCare.Application.Medications.Commands.UpdateMedication;
using HealthCare.Application.Medications.Queries.GetMedicationCompliance;
using HealthCare.Application.Medications.Queries.GetMedicationSchedules;
using HealthCare.Application.Medications.Queries.GetMedications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class MedicationsController : BaseController
{
    // ─── Medications ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetMedications(
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetMedicationsQuery(activeOnly), ct));

    [HttpPost]
    public async Task<IActionResult> CreateMedication(
        [FromBody] CreateMedicationCommand command,
        CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetMedications), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMedication(
        Guid id,
        [FromBody] UpdateMedicationCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command with { Id = id }, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMedication(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteMedicationCommand(id), ct);
        return NoContent();
    }

    // ─── Schedules ───────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/schedules")]
    public async Task<IActionResult> AddSchedule(
        Guid id,
        [FromBody] AddMedicationScheduleCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command with { MedicationId = id }, ct));

    [HttpDelete("schedules/{scheduleId:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId, CancellationToken ct)
    {
        await Sender.Send(new DeleteMedicationScheduleCommand(scheduleId), ct);
        return NoContent();
    }

    // ─── Logs (today's schedule) ─────────────────────────────────────────────

    [HttpGet("logs")]
    public async Task<IActionResult> GetTodayLogs(
        [FromQuery] DateOnly? date = null,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetMedicationSchedulesQuery(date), ct));

    [HttpPost("logs/{logId:guid}/taken")]
    public async Task<IActionResult> MarkTaken(Guid logId, CancellationToken ct)
        => Ok(await Sender.Send(new LogMedicationTakenCommand(logId), ct));

    [HttpPost("logs/{logId:guid}/skipped")]
    public async Task<IActionResult> MarkSkipped(
        Guid logId,
        [FromBody] SkipRequest? body,
        CancellationToken ct)
        => Ok(await Sender.Send(new LogMedicationSkippedCommand(logId, body?.SkipReason), ct));

    // ─── Compliance ──────────────────────────────────────────────────────────

    [HttpGet("compliance")]
    public async Task<IActionResult> GetCompliance(
        [FromQuery] int weeksBack = 4,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetMedicationComplianceQuery(weeksBack), ct));
}

public sealed record SkipRequest(string? SkipReason);
