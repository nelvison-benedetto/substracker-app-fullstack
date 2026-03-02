using MediatR;
using SubSnap.Application.Ports.Persistence;

namespace SubSnap.Application.Common.Behaviors;

/*
 * eviti di fare SaveChangesAsync in ogni handler, ma lo fai in un unico punto centrale, con questo pipeline behavior! cosi handler diventa pure orchestration.
  
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

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _uow;

    public TransactionBehavior(IUnitOfWork uow)
    {
        _uow = uow;
    }
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next(); //handler eseguito, ma non ancora salvato su db

        await _uow.SaveChangesAsync(ct); //!!FA SAVECHANGES, QUINDI ORA SEE EfUnitOfWork.cs(qui accadra tutto il outboxpattern: register user + send email in 1 sola transazione, 100% secure affidabilità.)!!!!
        //see User.cs  transactionbehavior.cs  efunitofwork.cs  outboxprocessor.cs

        return response;
    }
}