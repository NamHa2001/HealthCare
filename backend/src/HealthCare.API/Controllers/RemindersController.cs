using HealthCare.Application.Reminders.Commands.CancelReminder;
using HealthCare.Application.Reminders.Commands.CreateReminder;
using HealthCare.Application.Reminders.Queries.GetReminders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class RemindersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetReminders(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
        => Ok(await Sender.Send(new GetRemindersQuery(status, from, to), ct));

    [HttpPost]
    public async Task<IActionResult> CreateReminder(
        [FromBody] CreateReminderCommand command,
        CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetReminders), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelReminder(Guid id, CancellationToken ct)
    {
        await Sender.Send(new CancelReminderCommand(id), ct);
        return NoContent();
    }
}
