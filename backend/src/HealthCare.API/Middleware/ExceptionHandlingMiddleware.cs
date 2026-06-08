using System.Net;
using System.Text.Json;
using HealthCare.Application.Common.Exceptions;

namespace HealthCare.API.Middleware;

public class ExceptionHandlingMiddleware
{
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
        var (status, message, errors) = ex switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation failed", ve.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, (IDictionary<string, string[]>?)null),
            ForbiddenException fe => (HttpStatusCode.Forbidden, fe.Message, null),
            ConflictException ce => (HttpStatusCode.Conflict, ce.Message, null),
            _ => (HttpStatusCode.InternalServerError, "Đã xảy ra lỗi.", null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(new
        {
            status = (int)status,
            message,
            errors
        });

        await context.Response.WriteAsync(body);
    }
}
