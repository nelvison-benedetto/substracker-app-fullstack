using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SubSnap.Application.Common.Behaviors;

/*
 * MONITORA TUTTO! come fa Uber. se un handler impiega troppo tempo, lo logga come warning.
 */
public sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning(
                "Slow request {Request} took {Time}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}