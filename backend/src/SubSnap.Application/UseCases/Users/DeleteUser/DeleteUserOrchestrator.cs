using MediatR;

namespace SubSnap.Application.UseCases.Users.DeleteUser;

//wrappa l'handler, E' ENTRY POINT application!! è chiamato da plugin MediatR, dice 'Io sono il punto di ingresso dell’Application Layer per questo use case.'. 
//QUINDI NEL CONTROLLER FAI  await _gusOrchestrator.Execute(ct);, NO await _gusOrchestrator.Execute(ct); xk poi difficile cambiare orchestrazione, MEGLIO SE IL CONTROLLER NON CONOSCE mediatr!!
//Controller → Orchestrator → MediatR pipelibe behviors → Handler

public sealed class DeleteUserOrchestrator
{
    private readonly IMediator _mediator;

    public DeleteUserOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Execute(
        DeleteUserCommand command,
        CancellationToken ct = default)
    {
        return _mediator.Send(command, ct);  //QUI PARTE TUTTA LA PIPELINE mediatr cioe TUTTI I BEHAVIORS...fino all'handler!!
    }
}
