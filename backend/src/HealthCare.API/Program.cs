using HealthCare.API.Extensions;
using HealthCare.API.Middleware;
using HealthCare.Application;
using HealthCare.Infrastructure;
using HealthCare.Infrastructure.Persistence;
using HealthCare.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}
catch { /* already initialised in a previous test host invocation */ }

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<HealthCare.API.Filters.ApiResponseFilter>();
        })
        .AddJsonOptions(o =>
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

    // Lỗi model-binding / [ApiController] auto-validation → envelope lỗi chuẩn SRS §8.2.
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(kv => kv.Value!.Errors.Count > 0)
                .SelectMany(kv => kv.Value!.Errors.Select(e => new HealthCare.API.Models.ApiErrorDetail
                {
                    Field = kv.Key,
                    Message = string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Giá trị không hợp lệ." : e.ErrorMessage
                }))
                .ToList();

            var response = new HealthCare.API.Models.ApiErrorResponse
            {
                Success = false,
                Error = new HealthCare.API.Models.ApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = "Dữ liệu không hợp lệ.",
                    Details = details
                }
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Health+ API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new()
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Nhập JWT token: Bearer {token}"
        });
        c.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                []
            }
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Cho phép TẤT CẢ mọi origin
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    builder.Services.AddHealthChecks();

    // Clean Architecture layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApiAuthentication(builder.Configuration);

    var app = builder.Build();

    // Run migrations + seed data (idempotent, skipped for non-relational providers e.g. InMemory in tests)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
            await RoleSeeder.SeedAsync(db);
            await PermissionSeeder.SeedAsync(db);
            await DrugCatalogSeeder.SeedAsync(db);
            await VaccineCatalogSeeder.SeedAsync(db);
        }
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Health+ API v1"));
    }

    app.UseCors("AllowFrontend");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseMiddleware<CurrentUserMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Health+ API đang khởi động...");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "API khởi động thất bại");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
