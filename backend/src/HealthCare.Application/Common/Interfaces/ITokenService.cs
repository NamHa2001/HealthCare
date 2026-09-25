using HealthCare.Domain.Entities.Auth;

namespace HealthCare.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);

    /// <summary>BUG-11c: nguồn duy nhất cho thời hạn access token (giây) — tránh hardcode lệch config ở handler.</summary>
    int AccessTokenExpirySeconds { get; }
}