using FluentValidation;
using MediatR;

namespace SubSnap.Application.Common.Behaviors;

/*
MediarR lavora come un middleware .net, ma dentro prj .Application! 
intercetta datas prima che arrivano agli HANDLERs (di UseCases).

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

public sealed class ValidationBehavior<TRequest, TResponse>  
    : IPipelineBehavior<TRequest, TResponse>  //intercetta ogni command/query MediatR
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;  //.net cosi automaticmnete trova e.g. RegisterUserValidator : AbstractValidator<RUCommand> (in .api.validators.XXXfile) e LO INIETTA!!

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validators.Any())  //SE ESISTONO VALIDATORS per il command intercettato...
        {
            var context = new ValidationContext<TRequest>(request); //fluentvalidator lavora su questo context

            var results = await Task.WhenAll(  //validazione in parallelo (contemporaneamente) di tutti i validators. in parallelo lo utilizzo anche x i READS sul db.
                _validators.Select(v => v.ValidateAsync(context, ct)));

            var failures = results
                .SelectMany(r => r.Errors) //raccolgo tutti gli errori di validazione
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures); //se trova qualche errore, allora lo lancia e VERRA INTERCETTATO DA ExceptionBehavior (che lo gestisce e restituisce 400 BadRequest al client)
        }

        return await next(); //continua lungo la pipeline...
    }
}