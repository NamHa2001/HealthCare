using HealthCare.Application.HealthProfiles.Commands.UpdateHealthProfile;
using HealthCare.Application.HealthProfiles.Queries.GetHealthProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class HealthProfilesController : BaseController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        => Ok(await Sender.Send(new GetHealthProfileQuery(), ct));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateHealthProfileCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return Ok();
    }
}
