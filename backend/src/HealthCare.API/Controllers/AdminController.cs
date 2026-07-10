using HealthCare.Application.Admin.Commands.ResetUserPassword;
using HealthCare.Application.Admin.Commands.ToggleUserActive;
using HealthCare.Application.Doctors.Commands.VerifyDoctor;
using HealthCare.Application.Doctors.Queries;
using HealthCare.Application.Admin.Queries.ExportUsers;
using HealthCare.Application.Admin.Queries.GetAuditLogs;
using HealthCare.Application.Admin.Queries.GetSystemStats;
using HealthCare.Application.Admin.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

public record ResetPasswordRequest(string NewPassword);
public record VerificationReasonRequest(string Reason);

[Authorize(Roles = "admin")]
public class AdminController : BaseController
{
    // ─── System Stats ─────────────────────────────────────────────────────────

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
        => Ok(await Sender.Send(new GetSystemStatsQuery(), ct));

    // ─── Users ────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetUsersQuery(search, isActive, page, pageSize), ct));

    [HttpPut("users/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleUserActive(Guid id, CancellationToken ct)
    {
        await Sender.Send(new ToggleUserActiveCommand(id), ct);
        return NoContent();
    }

    [HttpPut("users/{id:guid}/reset-password")]
    public async Task<IActionResult> ResetUserPassword(
        Guid id,
        [FromBody] ResetPasswordRequest body,
        CancellationToken ct)
    {
        await Sender.Send(new ResetUserPasswordCommand(id, body.NewPassword), ct);
        return NoContent();
    }

    [HttpGet("users/export")]
    public async Task<IActionResult> ExportUsers(CancellationToken ct)
    {
        var csv = await Sender.Send(new ExportUsersQuery(), ct);
        return File(csv, "text/csv", $"users-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    // ─── Doctor Verifications ────────────────────────────────────────────────

    [HttpGet("doctor-verifications")]
    public async Task<IActionResult> GetDoctorVerifications(
        [FromQuery] string? status, CancellationToken ct)
        => Ok(await Sender.Send(new GetDoctorVerificationsQuery(status), ct));

    [HttpGet("doctor-verifications/{id:guid}")]
    public async Task<IActionResult> GetDoctorVerificationDetail(Guid id, CancellationToken ct)
        => Ok(await Sender.Send(new GetDoctorVerificationDetailQuery(id), ct));

    [HttpPost("doctor-verifications/{id:guid}/approve")]
    public async Task<IActionResult> ApproveDoctor(Guid id, CancellationToken ct)
    {
        await Sender.Send(new ApproveDoctorCommand(id), ct);
        return NoContent();
    }

    [HttpPost("doctor-verifications/{id:guid}/reject")]
    public async Task<IActionResult> RejectDoctor(
        Guid id, [FromBody] VerificationReasonRequest body, CancellationToken ct)
    {
        await Sender.Send(new RejectDoctorCommand(id, body.Reason), ct);
        return NoContent();
    }

    [HttpPost("doctor-verifications/{id:guid}/suspend")]
    public async Task<IActionResult> SuspendDoctor(
        Guid id, [FromBody] VerificationReasonRequest body, CancellationToken ct)
    {
        await Sender.Send(new SuspendDoctorCommand(id, body.Reason), ct);
        return NoContent();
    }

    // ─── Audit Logs ───────────────────────────────────────────────────────────

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? resource,
        [FromQuery] string? eventType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
        => Ok(await Sender.Send(new GetAuditLogsQuery(resource, eventType, page, pageSize), ct));
}
