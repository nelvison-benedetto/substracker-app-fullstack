using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SubSnap.Application.Common.Behaviors;

/*
ora ogni volta che un handler viene usato nel log appare 
        Handling RegisterUserCommand
        Handled RegisterUserCommand in 45ms

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

public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "Handling {RequestName} {@Request}",  //@ serializza obj
            requestName,
            request);
        //devi loggare sempre in questo modo, senza '+' xk cosi e.g.OpenTelemetry riesce a parsare i log e a creare metriche!

        var sw = Stopwatch.StartNew();  //misura il tempo di esecuzione dell'handler

        var response = await next();  //continua la pipeline...

        sw.Stop();

        _logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms",
            requestName,
            sw.ElapsedMilliseconds);

        return response;
    }
}