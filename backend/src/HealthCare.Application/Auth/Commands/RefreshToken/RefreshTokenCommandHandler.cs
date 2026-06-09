using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using RefreshTokenEntity = HealthCare.Domain.Entities.Auth.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _token;

    public RefreshTokenCommandHandler(IApplicationDbContext db, ITokenService token)
    {
        _db = db;
        _token = token;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _db.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, ct);

        if (existing is null || !existing.IsActive)
            throw new ForbiddenException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var newValue = _token.GenerateRefreshToken();
        existing.Revoke(newValue);

        var roles = existing.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newRefresh = RefreshTokenEntity.Create(existing.UserId, newValue, 7, request.IpAddress);
        _db.RefreshTokens.Add(newRefresh);

        await _db.SaveChangesAsync(ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = _token.GenerateAccessToken(existing.User, roles),
            RefreshToken = newValue,
            ExpiresIn = 900,
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
}