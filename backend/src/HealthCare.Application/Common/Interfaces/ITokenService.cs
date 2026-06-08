using HealthCare.Domain.Entities.Auth;

namespace HealthCare.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}