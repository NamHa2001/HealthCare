using HealthCare.Application.Auth.Commands.ChangePassword;
using HealthCare.Application.Auth.Commands.ForgotPassword;
using HealthCare.Application.Auth.Commands.UpdateProfile;
using HealthCare.Application.Auth.Commands.Login;
using HealthCare.Application.Auth.Commands.Logout;
using HealthCare.Application.Auth.Commands.RefreshToken;
using HealthCare.Application.Auth.Commands.Register;
using HealthCare.Application.Auth.Commands.ResetPassword;
using HealthCare.Application.Auth.Commands.VerifyEmail;
using HealthCare.Application.Auth.Queries.ExportUserData;
using HealthCare.Application.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HealthCare.API.Controllers;

public class AuthController : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return Ok(await Sender.Send(command with { IpAddress = ip }, ct));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        return Ok(await Sender.Send(command with { IpAddress = ip }, ct));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();
        return Ok(await Sender.Send(command with { UserId = id }, ct));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();
        return Ok(await Sender.Send(new GetCurrentUserQuery(id), ct));
    }

    /// <summary>SRS §9.5 — Export dữ liệu cá nhân (Nghị định 13/2023).</summary>
    [Authorize]
    [HttpGet("me/export")]
    public async Task<IActionResult> ExportData(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();

        var data = await Sender.Send(new ExportUserDataQuery(id), ct);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var filename = $"healthplus-data-{DateTime.UtcNow:yyyyMMdd}.json";
        return File(bytes, "application/json", filename);
    }
}