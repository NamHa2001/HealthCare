using System.Security.Cryptography;
using HealthCare.API.Middleware;
using HealthCare.Application.Common.Interfaces;
using HealthCare.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthCare.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Resolve JWT key lazily at first request (not at service registration)
        // so WebApplicationFactory config overrides are applied in time.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, cfg) =>
            {
                var publicKeyPath = cfg["Jwt:PublicKeyPath"];
                if (string.IsNullOrEmpty(publicKeyPath))
                    throw new InvalidOperationException("Jwt:PublicKeyPath configuration is missing.");

                var pem = PemKeyLoader.ReadPem(publicKeyPath);
                var rsa = RSA.Create();
                rsa.ImportFromPem(pem);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = cfg["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }
}
