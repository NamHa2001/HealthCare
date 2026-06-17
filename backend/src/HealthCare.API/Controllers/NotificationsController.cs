using HealthCare.Application.Notifications.Commands.SubscribePush;
using HealthCare.Application.Notifications.Commands.UnsubscribePush;
using HealthCare.Application.Notifications.Commands.UpdateNotificationPreference;
using HealthCare.Application.Notifications.Queries.GetNotificationPreference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class NotificationsController : BaseController
{
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken ct)
        => Ok(await Sender.Send(new GetNotificationPreferenceQuery(), ct));

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] UpdateNotificationPreferenceCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("push-subscribe")]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribePushCommand command,
        CancellationToken ct)
    {
        var id = await Sender.Send(command, ct);
        return Ok(new { id });
    }

    [HttpDelete("push-subscribe/{subscriptionId:guid}")]
    public async Task<IActionResult> Unsubscribe(Guid subscriptionId, CancellationToken ct)
    {
        await Sender.Send(new UnsubscribePushCommand(subscriptionId), ct);
        return NoContent();
    }
}
