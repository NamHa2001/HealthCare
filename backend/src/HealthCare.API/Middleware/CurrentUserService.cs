using System.Security.Claims;
using HealthCare.Application.Common.Interfaces;

namespace HealthCare.API.Middleware;

public class CurrentUserService : ICurrentUser
{
    private ClaimsPrincipal? _user;

    public void SetUser(ClaimsPrincipal user) => _user = user;

    public Guid? UserId
    {
        get
        {
            var val = _user?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _user?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(val, out var id) ? id : null;
        }
    }

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);
    public IEnumerable<string> Roles => _user?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];
    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;
}