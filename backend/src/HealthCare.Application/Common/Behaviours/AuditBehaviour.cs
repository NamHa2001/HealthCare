using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthCare.Application.Common.Behaviours;

public class AuditBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<AuditBehaviour<TRequest, TResponse>> _logger;

    public AuditBehaviour(ILogger<AuditBehaviour<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        if (sw.ElapsedMilliseconds > 500)
            _logger.LogWarning("Long running request: {Request} ({Elapsed}ms)", typeof(TRequest).Name, sw.ElapsedMilliseconds);

        return response;
    }
}