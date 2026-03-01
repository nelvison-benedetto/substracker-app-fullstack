using MediatR;
using Microsoft.Extensions.Logging;
using SubSnap.Core.Domain.Exceptions;

namespace SubSnap.Application.Common.Behaviors;

/*
 * .application level  exception pipeline. ORA NON DEVI PIU FARE I TRY/CATCH negli handler. gestione centralizzata automatica w plugin MediatR. se un handler lancia un'eccezione, questa viene catturata da questo pipeline behavior, che decide come loggarla e rilanciarla.
 * services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(XXXBehavior<,>));

depedencyinjection.cs (.application level)
quando fai
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
 */
public sealed class ExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<ExceptionBehavior<TRequest, TResponse>> _logger;
    public ExceptionBehavior(ILogger<ExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)  // se è un'eccezione di dominio, loggo come warning, altrimenti come error. le eccezioni di dominio sono prevedibili, non sono errori di sistema, ma errori di logica del dominio. quindi non voglio spammare i log con errori che sono normali, ma voglio comunque loggarli per capire cosa è successo.
        {
            _logger.LogWarning(ex,
                "Domain error for {Request}", typeof(TRequest).Name);  //coloro in giallo, è un warning, non un error. è un'eccezione di dominio, non un'eccezione di sistema.
            throw; // rilancio l'eccezione, altrimenti il comportamento successivo non sa che è successo un errore. se non rilancio, il comportamento successivo pensa che tutto sia andato bene, e continua a eseguire il codice, ma in realtà c'è stato un errore.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled error for {Request}", typeof(TRequest).Name);  //coloro in rosso, è un error, non un warning. è un'eccezione di sistema, non un'eccezione di dominio.
            throw;
        }
    }
}
