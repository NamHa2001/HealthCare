using HealthCare.Application.Features.Auth.Commands.ChangePassword;
using HealthCare.Application.Features.Auth.Commands.ForgotPassword;
using HealthCare.Application.Features.Auth.Commands.Login;
using HealthCare.Application.Features.Auth.Commands.Logout;
using HealthCare.Application.Features.Auth.Commands.RefreshToken;
using HealthCare.Application.Features.Auth.Commands.Register;
using HealthCare.Application.Features.Auth.Commands.ResetPassword;
using HealthCare.Application.Features.Auth.Commands.VerifyEmail;
using HealthCare.Application.Features.Auth.Queries.GetCurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
        => Ok(await Sender.Send(command, ct));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id)) return Unauthorized();
        return Ok(await Sender.Send(new GetCurrentUserQuery(id), ct));
    }
}