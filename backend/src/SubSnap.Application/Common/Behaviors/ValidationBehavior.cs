using FluentValidation;
using MediatR;

namespace SubSnap.Application.Common.Behaviors;

/*
 * MediarR lavora come un middleware .net, ma dentro prj .Application! 
 * intercetta datas prima che arrivano agli HANDLERs (di UseCases).
 * 
 * await _mediator.Send(command) -> passa attraverso questa pipeline di comportamenti (tra cui questo ValidationBehavior) -> arriva all'handler target del comando.
 */

public sealed class ValidationBehavior<TRequest, TResponse>  
    : IPipelineBehavior<TRequest, TResponse>  //intercetta ogni request mediatr
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;  //.net cosi automaticmnete trova e.g. RegisterUserValidator : AbstractValidator<RUCommand> (.api.validators.X) e LO INIETTA

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request); //fluentvalidator lavora su questo context

            var results = await Task.WhenAll(  //validazione in parallelo (contemporaneamente), gia usato anche x reads dal db in parallelo.
                _validators.Select(v => v.ValidateAsync(context, ct)));

            var failures = results
                .SelectMany(r => r.Errors) //raccolgo tutti gli errori di validazione
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next(); //continua lungo la pipeline (cioe ora va all'HANDLER target!!)!
    }
}