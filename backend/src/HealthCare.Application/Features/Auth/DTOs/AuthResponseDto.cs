namespace HealthCare.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public int ExpiresIn { get; init; }
    public UserInfoDto User { get; init; } = null!;
}