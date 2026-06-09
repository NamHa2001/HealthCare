using HealthCare.Application.Common.Exceptions;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Application.Common.Models;
using HealthCare.Application.Auth.DTOs;
using RefreshTokenEntity = HealthCare.Domain.Entities.Auth.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _token;

    public LoginCommandHandler(IApplicationDbContext db, ITokenService token)
    {
        _db = db;
        _token = token;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ForbiddenException("Email hoặc mật khẩu không đúng.");    

        if (!user.IsActive)
            throw new ForbiddenException("Tài khoản đã bị vô hiệu hóa.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _token.GenerateAccessToken(user, roles);
        var refreshTokenValue = _token.GenerateRefreshToken();

        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, 7, request.IpAddress);
        _db.RefreshTokens.Add(refreshToken);

        user.RecordLogin();
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