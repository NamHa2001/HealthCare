using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HealthCare.Infrastructure.Services.Auth;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    // BUG-11b: TokenService là scoped (mới mỗi request) nhưng key PEM không đổi khi app đang chạy
    // — cache tĩnh theo đường dẫn file để tránh đọc lại disk + parse PEM mỗi lần sign/verify.
    private static readonly ConcurrentDictionary<string, RSA> _keyCache = new();

    public TokenService(IConfiguration config) => _config = config;

    public int AccessTokenExpirySeconds => int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15") * 60;

    private static RSA LoadKey(string path) =>
        _keyCache.GetOrAdd(path, p =>
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(PemKeyLoader.ReadPem(p));
            return rsa;
        });

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var rsa = LoadKey(_config["Jwt:PrivateKeyPath"]!);
        var key = new RsaSecurityKey(rsa);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AccessTokenExpirySeconds),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public Guid? ValidateAccessToken(string token)
    {
        try
        {
            var rsa = LoadKey(_config["Jwt:PublicKeyPath"]!);

            var handler = new JwtSecurityTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var sub = result.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
