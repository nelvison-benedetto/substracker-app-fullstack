using FluentValidation;
using MediatR;

namespace SubSnap.Application.Common.Behaviors;

/*
MediarR lavora come un middleware .net, ma dentro prj .Application! 
intercetta datas prima che arrivano agli HANDLERs (di UseCases).

depedencyinjection.cs (.application level)
quando fai nel usercontroller.cs
await _mediator.Send(command) la pipeline (grazie a method Handle?) è
PIPELINE
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
//see validationbehaviour.cs  dependencyinjection.cs  validationextensions.cs

public sealed class ValidationBehavior<TRequest, TResponse>  
    : IPipelineBehavior<TRequest, TResponse>  //intercetta ogni command/query MediatR
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    /*
        When MediatR executes:
        await _mediator.Send(new SomeCommand(...));
        it builds the pipeline:
        ValidationBehavior<SomeCommand, TResult>
        The DI container automatically resolves:
        IEnumerable<IValidator<SomeCommand>>
        FluentValidation scans registered assemblies and finds
        all validators implementing:  IValidator<SomeCommand>
        Example:
        RegisterUserValidator : AbstractValidator<RegisterUserCommand>
        These validators are automatically injected here.
     */
    //.net automaticamente trova tutti i validators collegati a X command (TRequest), e li inietta.
    //e.g. RUCommand (command usato in registeruservalidator.cs) --> trova RegisterUserValidator ( .api/validators/RegisterUserValidator.cs) e LO INIETTA!!

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

            var results = await Task.WhenAll(  //validazione in parallelo (contemporaneamente) di tutti i validators. in parallelo. invece x i READS sul db uso le batch query, dicisamente mooolto meglio delle query in parallelo.
                _validators.Select(v => v.ValidateAsync(context, ct)));  //EXECUTE LA VALIDAZIONE del validator target. ValidateAsync() deriva da AbstractValidator<T>, ed esegue le rules definite nel validator target.

            var failures = results
                .SelectMany(r => r.Errors) //raccolgo tutti gli errori di validazione
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures); //!!INTERCETTATO DA ExceptionBehavior.cs (che lo gestisce e restituisce 400 BadRequest al client)
        }

        return await next(); //continua lungo la PIPELINE(SCORRI IN ALTO w mouse)
    }
}