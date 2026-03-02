using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SubSnap.Application.Common.Behaviors;

/*
 * MONITORA TUTTO! come fa Uber. se un handler impiega troppo tempo, lo logga come warning.
 * cosi identifichi gli handlers lenti!
  
 depedencyinjection.cs (.application level)
quando fai nel usercontroller.cs
await _mediator.Send(command) la pipeline (grazie a method Handle) è
 Controller
   ↓
ValidationBehavior
   ↓
LoggingBehavior
   ↓
PerformanceBehavior
   ↓
TransactionBehavior
   ↓
ExceptionBehavior
   ↓
Handler
//plugin MediatR costruisce dinamicamente la pipeline usando reflrection, quindi cerca sermpe Handle(...) !!
COME FUNZIONA CON NEXT/RETURN RESPONSE NELLA PIPELINE
TransactionBehavior entra
↓
await next()
    ↓
    RUHandler.Handle()
        AddAsync(user)
        (NO SAVE)
    ↑ ritorna
↓
SaveChangesAsync()   ← QUI
↓
response
//quindi transactionbehavior(che lancierà EFunitofwork!) circonda exceptionbehavior che circonda a sua volta handler!! cipolla.
 */
public sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();  //avvia il cronometro

        var response = await next();

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > 500)  // se impiega più di 500ms, logga un warning
        {
            _logger.LogWarning(
                "Slow request {Request} took {Time}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}