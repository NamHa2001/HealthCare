using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using RefreshTokenEntity = HealthCare.Domain.Entities.Auth.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _token;
    private readonly IEmailService _email;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext db,
        ITokenService token,
        IEmailService email,
        ILogger<LoginCommandHandler> logger)
    {
        _db = db;
        _token = token;
        _email = email;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        // Ambiguous error — don't reveal whether email exists
        if (user is null)
            throw new ForbiddenException("Email hoặc mật khẩu không đúng.");

        // SRS §1.2: lock check
        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling((user.LockedUntil!.Value - DateTime.UtcNow).TotalHours);
            throw new ForbiddenException($"Tài khoản tạm khóa. Thử lại sau {remaining} giờ.");
        }

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị vô hiệu hóa.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            var wasUnlocked = !user.IsLockedOut();
            user.RecordFailedLogin();
            await _db.SaveChangesAsync(ct);

            // Send email warning when account becomes locked (10th failure)
            if (wasUnlocked && user.IsLockedOut())
            {
                try { await _email.SendAccountLockedAsync(user.Email, user.FirstName, ct); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not send lock email to {Email}", user.Email); }
            }

            throw new ForbiddenException("Email hoặc mật khẩu không đúng.");
        }

        user.ResetFailedLogin();
        user.RecordLogin();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _token.GenerateAccessToken(user, roles);
        var refreshTokenValue = _token.GenerateRefreshToken();

        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, 7, request.IpAddress);
        _db.RefreshTokens.Add(refreshToken);

        await _db.SaveChangesAsync(ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = 900,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailVerified = user.IsEmailVerified,
                Roles = roles
            }
        });
    }
}