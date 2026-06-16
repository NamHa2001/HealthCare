using HealthCare.Application.Common.Interfaces;
using HealthCare.Infrastructure.Persistence;
using HealthCare.Infrastructure.Services.Auth;
using HealthCare.Infrastructure.Services.Ocr;
using HealthCare.Infrastructure.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace HealthCare.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // EF Core — SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        // Auth services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEncryptionService, EncryptionService>();

        // MinIO — file storage
        var storageUri = new Uri(config["Storage:Endpoint"] ?? "http://localhost:9000");
        services.AddMinio(client => client
            .WithEndpoint(storageUri.Host, storageUri.Port)
            .WithCredentials(
                config["Storage:AccessKey"] ?? "minioadmin",
                config["Storage:SecretKey"] ?? "minioadmin")
            .WithSSL(storageUri.Scheme == "https"));
        services.AddScoped<IFileStorageService, MinioFileStorageService>();

        // OCR services — Google Vision (primary) + EasyOCR (fallback) + Proxy (circuit breaker)
        services.AddHttpClient();
        services.AddSingleton<PrescriptionExtractor>();
        services.AddScoped<GoogleVisionOcrService>();
        services.AddScoped<EasyOcrService>();
        services.AddScoped<IOcrService, OcrServiceProxy>();

        return services;
    }
}
