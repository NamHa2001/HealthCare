using HealthCare.Application.Doctors.Links.Commands;
using HealthCare.Application.Doctors.Links.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

public record InviteDoctorRequest(Guid DoctorUserId, Guid HealthProfileId, List<string> ConsentScope);
public record AcceptLinkRequest(List<string>? ConsentScope);

/// <summary>Phía bệnh nhân: quản lý liên kết với bác sĩ (DOCTOR_PORTAL.md §5).</summary>
[Authorize]
public class DoctorLinksController : BaseController
{
    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UserAgent => Request.Headers.UserAgent.ToString();

    [HttpGet]
    public async Task<IActionResult> GetMyLinks(CancellationToken ct)
        => Ok(await Sender.Send(new GetMyDoctorLinksQuery(), ct));

    /// <summary>Bệnh nhân chủ động mời bác sĩ — consent (kèm phạm vi) ghi ngay lúc mời.</summary>
    [HttpPost]
    public async Task<IActionResult> InviteDoctor([FromBody] InviteDoctorRequest body, CancellationToken ct)
        => Ok(await Sender.Send(
            new InviteDoctorCommand(body.DoctorUserId, body.HealthProfileId, body.ConsentScope, ClientIp, UserAgent), ct));

    /// <summary>Chấp nhận lời mời — bệnh nhân cung cấp phạm vi (khi bác sĩ mời),
    /// hoặc bác sĩ xác nhận (khi bệnh nhân mời, scope bỏ trống).</summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptLinkRequest body, CancellationToken ct)
    {
        await Sender.Send(new AcceptDoctorLinkCommand(id, body.ConsentScope ?? [], ClientIp, UserAgent), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct)
    {
        await Sender.Send(new RejectDoctorLinkCommand(id), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        await Sender.Send(new RevokeDoctorLinkCommand(id), ct);
        return NoContent();
    }
}
