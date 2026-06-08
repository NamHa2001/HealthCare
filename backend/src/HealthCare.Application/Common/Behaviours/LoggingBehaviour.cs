using HealthCare.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger, ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {Request} by {User}", name, _currentUser.Email ?? "anonymous");
        var response = await next();
        _logger.LogInformation("Handled {Request}", name);
        return response;
    }
}