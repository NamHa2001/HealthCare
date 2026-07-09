using HealthCare.Application.Sharing.Commands.CreateShareGrant;
using HealthCare.Application.Sharing.Commands.RevokeShareGrant;
using HealthCare.Application.Sharing.Queries.GetMyShareGrants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class ShareGrantsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShareGrantCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpGet]
    public async Task<IActionResult> GetMyGrants([FromQuery] Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetMyShareGrantsQuery(profileId), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        await Sender.Send(new RevokeShareGrantCommand(id), ct);
        return NoContent();
    }
}
