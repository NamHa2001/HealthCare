using HealthCare.Application.Common.Interfaces;
using HealthCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthCare.Integration.Tests;

public class HealthPlusWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var keysDir = FindKeysDirectory();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Server=.;Database=Test;TrustServerCertificate=True;Integrated Security=true;",
                ["Jwt:PrivateKeyPath"]    = Path.Combine(keysDir, "jwt_private.pem"),
                ["Jwt:PublicKeyPath"]     = Path.Combine(keysDir, "jwt_public.pem"),
                ["Jwt:Issuer"]            = "https://test.healthplus.local",
                ["Jwt:Audience"]          = "https://test.healthplus.local",
                ["Storage:Endpoint"]      = "http://localhost:9000",
                ["Storage:AccessKey"]     = "minioadmin",
                ["Storage:SecretKey"]     = "minioadmin",
                ["Storage:BucketDocuments"] = "health-documents",
                ["Email:Host"]            = "localhost",
                ["Email:Port"]            = "1025",
                ["Email:Username"]        = "",
                ["Email:Password"]        = "",
                ["Email:FromAddress"]     = "test@healthplus.local",
                ["Email:FromName"]        = "Health+ Test",
                ["Encryption:Key"]        = "+pz65nUQzupxplFHdIiRk1GAnZnWZPqsOSPzhUOnhlY=",
                ["Fcm:ProjectId"]         = "test-project",
                ["Ocr:EasyOcrUrl"]        = "http://localhost:5001",
                ["Ocr:GoogleVisionApiKey"] = "",
                ["Hangfire:DashboardUser"] = "admin",
                ["Hangfire:DashboardPass"] = "admin",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove all EF Core related registrations to avoid dual-provider error
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IApplicationDbContext>();

            // Also remove any IDbContextOptions extensions registered by AddDbContext
            var efDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("EntityFramework") == true
                         || d.ServiceType.FullName?.Contains("DbContext") == true)
                .ToList();
            foreach (var d in efDescriptors)
                services.Remove(d);

            var dbName = _dbName;
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            services.AddScoped<IApplicationDbContext>(
                sp => sp.GetRequiredService<ApplicationDbContext>());
        });
    }

    private static string FindKeysDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "HealthCare.API", "keys");
            if (Directory.Exists(candidate)) return candidate;

            var local = Path.Combine(dir.FullName, "keys");
            if (Directory.Exists(local) && File.Exists(Path.Combine(local, "jwt_public.pem")))
                return local;

            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            $"Cannot find JWT keys directory. Searched from: {AppContext.BaseDirectory}");
    }
}
