using System.Net;
using System.Text.Json;
using HealthCare.API.Models;
using HealthCare.Application.Common.Exceptions;

namespace HealthCare.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Map exception → (HTTP status, error code, message, details) theo SRS §8.2.
        var (status, code, message, details) = ex switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                "VALIDATION_ERROR",
                ve.Message,
                ve.Errors
                    .SelectMany(kv => kv.Value.Select(m => new ApiErrorDetail { Field = kv.Key, Message = m }))
                    .ToList()),
            NotFoundException nfe => (HttpStatusCode.NotFound, "NOT_FOUND", nfe.Message, (List<ApiErrorDetail>?)null),
            ForbiddenException fe => (HttpStatusCode.Forbidden, "FORBIDDEN", fe.Message, null),
            ConflictException ce => (HttpStatusCode.Conflict, "CONFLICT", ce.Message, null),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "Đã xảy ra lỗi.", null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var response = new ApiErrorResponse
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
