using System.Security.Claims;
using HealthCare.Application.Common.Interfaces;

namespace HealthCare.API.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;
    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser is CurrentUserService service && context.User.Identity?.IsAuthenticated == true)
            service.SetUser(context.User);

        await _next(context);
    }
}
