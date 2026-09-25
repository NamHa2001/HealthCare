using HealthCare.Application.Common.Interfaces;
using HealthCare.Infrastructure.BackgroundJobs;
using HealthCare.Infrastructure.Persistence;
using HealthCare.Infrastructure.Persistence.Seeds;
using HealthCare.Infrastructure.Services.Auth;
using HealthCare.Infrastructure.Services.Notifications;
using HealthCare.Infrastructure.Services.Ocr;
using HealthCare.Infrastructure.Services.Storage;
using HealthCare.Infrastructure.Services.Vaccines;
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

        // File storage — "Minio" (mặc định, dev + docker) hoặc "Local" (ổ đĩa máy chủ, cho host không có MinIO)
        services.AddHttpContextAccessor();
        services.AddScoped<LocalFileStorageService>();
        if (string.Equals(config["Storage:Provider"], "Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService>(sp => sp.GetRequiredService<LocalFileStorageService>());
        }
        else
        {
            var storageUri = new Uri(config["Storage:Endpoint"] ?? "http://localhost:9000");
            services.AddMinio(client => client
                .WithEndpoint(storageUri.Host, storageUri.Port)
                .WithCredentials(
                    config["Storage:AccessKey"] ?? "minioadmin",
                    config["Storage:SecretKey"] ?? "minioadmin")
                .WithSSL(storageUri.Scheme == "https"));
            services.AddScoped<IFileStorageService, MinioFileStorageService>();
        }

        // OCR services — Google Vision (primary) + EasyOCR (fallback) + Proxy (circuit breaker)
        services.AddHttpClient();
        services.AddSingleton<PrescriptionExtractor>();
        services.AddScoped<GoogleVisionOcrService>();
        services.AddScoped<EasyOcrService>();
        services.AddScoped<IOcrService, OcrServiceProxy>();

        // Vaccine & Notification services
        services.AddScoped<IVaccineScheduleService, VaccineScheduleService>();
        services.AddSingleton<IVaccinePassportService, VaccinePassportService>();
        services.AddScoped<INotificationService, FcmNotificationService>();

        // Background jobs — chạy bởi JobSchedulerHostedService (mỗi 15' / hàng ngày)
        services.AddScoped<ReminderProcessorJob>();
        services.AddScoped<VaccineReminderJob>();
        services.AddScoped<DoctorDigestJob>();
        services.AddScoped<ShareGrantCleanupJob>();
        services.AddScoped<MedicationLogGeneratorJob>();
        services.AddHostedService<JobSchedulerHostedService>();

        return services;
    }

    public static async Task SeedDatabaseAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await RoleSeeder.SeedAsync(db);
        await PermissionSeeder.SeedAsync(db);
        await DrugCatalogSeeder.SeedAsync(db);
        await VaccineCatalogSeeder.SeedAsync(db);
    }
}
