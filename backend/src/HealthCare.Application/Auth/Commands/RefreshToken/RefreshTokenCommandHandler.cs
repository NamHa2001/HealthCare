using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using HealthCare.Application.Sharing.Common;
using RefreshTokenEntity = HealthCare.Domain.Entities.Auth.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _token;
    private readonly IEmailService _email;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IApplicationDbContext db, ITokenService token, IEmailService email, ILogger<RefreshTokenCommandHandler> logger)
    {
        _db = db;
        _token = token;
        _email = email;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = ShareTokens.Hash(request.Token);
        var existing = await _db.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (existing is null)
            throw new ForbiddenException("Refresh token không hợp lệ hoặc đã hết hạn.");

        // BUG-08: token đã bị revoke (nghĩa là đã dùng để refresh trước đó) nhưng lại bị dùng lần nữa
        // — dấu hiệu kinh điển của việc bị đánh cắp (cả kẻ trộm và nạn nhân cùng dùng chung 1 token cũ).
        // Coi như toàn bộ chuỗi token thay thế kể từ đây đã bị lộ, revoke hết + cảnh báo user.
        if (existing.IsRevoked)
        {
            await RevokeDescendantChainAsync(existing, ct);
            await _db.SaveChangesAsync(ct);

            try
            {
                await _email.SendSecurityAlertAsync(
                    existing.User.Email, existing.User.FirstName,
                    "phát hiện refresh token đã thu hồi bị sử dụng lại", ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không gửi được email cảnh báo bảo mật tới {Email}", existing.User.Email);
            }

            throw new ForbiddenException("Refresh token không hợp lệ hoặc đã hết hạn.");
        }

        if (!existing.IsActive)
            throw new ForbiddenException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var newValue = _token.GenerateRefreshToken();
        existing.Revoke(ShareTokens.Hash(newValue));

        var roles = existing.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newRefresh = RefreshTokenEntity.Create(existing.UserId, ShareTokens.Hash(newValue), 7, request.IpAddress);
        _db.RefreshTokens.Add(newRefresh);

        await _db.SaveChangesAsync(ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = _token.GenerateAccessToken(existing.User, roles),
            RefreshToken = newValue,
            ExpiresIn = _token.AccessTokenExpirySeconds,
            User = new UserInfoDto
            {
                Id = existing.User.Id,
                Email = existing.User.Email,
                FirstName = existing.User.FirstName,
                LastName = existing.User.LastName,
                IsEmailVerified = existing.User.IsEmailVerified,
                Roles = roles
            }
        });
    }

    /// <summary>Đi theo dây chuyền ReplacedByTokenHash để revoke mọi token thay thế còn active.</summary>
    private async Task RevokeDescendantChainAsync(RefreshTokenEntity start, CancellationToken ct)
    {
        var nextHash = start.ReplacedByTokenHash;
        while (nextHash is not null)
        {
            var next = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == nextHash, ct);
            if (next is null) break;

            nextHash = next.ReplacedByTokenHash;
            if (!next.IsRevoked) next.Revoke();
        }
    }
}