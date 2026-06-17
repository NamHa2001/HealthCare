using HealthCare.Application.Sync.Commands.PushSync;
using HealthCare.Application.Sync.Queries.PullSync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class SyncController : BaseController
{
    [HttpPost("push")]
    public async Task<IActionResult> Push(
        [FromBody] PushSyncCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpGet("pull")]
    public async Task<IActionResult> Pull(
        [FromQuery] long since = 0,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new PullSyncQuery(since), ct));
}
