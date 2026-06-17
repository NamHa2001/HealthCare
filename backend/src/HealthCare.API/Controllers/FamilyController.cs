using HealthCare.Application.Family.Commands.AcceptInvite;
using HealthCare.Application.Family.Commands.AddFamilyMember;
using HealthCare.Application.Family.Commands.CreateFamilyGroup;
using HealthCare.Application.Family.Commands.InviteMember;
using HealthCare.Application.Family.Commands.RemoveFamilyMember;
using HealthCare.Application.Family.Queries.GetFamily;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class FamilyController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetFamily(CancellationToken ct)
        => Ok(await Sender.Send(new GetFamilyQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> CreateFamily(
        [FromBody] CreateFamilyGroupCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("members")]
    public async Task<IActionResult> AddMember(
        [FromBody] AddFamilyMemberCommand command,
        CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpDelete("members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid memberId, CancellationToken ct)
    {
        await Sender.Send(new RemoveFamilyMemberCommand(memberId), ct);
        return NoContent();
    }

    /// <summary>SRS §2.1 — Mời thành viên qua email.</summary>
    [HttpPost("invite")]
    public async Task<IActionResult> InviteMember([FromBody] InviteFamilyMemberCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return Ok(new { message = "Đã gửi lời mời." });
    }

    /// <summary>Chấp nhận lời mời qua token từ email.</summary>
    [HttpPost("invite/accept")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptFamilyInviteCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return Ok(new { message = "Đã tham gia nhóm gia đình." });
    }
}
