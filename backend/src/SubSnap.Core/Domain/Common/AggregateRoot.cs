namespace SubSnap.Core.Domain.Common;

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents  //esponi eventi come ReadOnlyCollection, cosi nessuno puo modificarli dall'esterno, ma solo leggerli
        => _events.AsReadOnly();

    protected void Raise(IDomainEvent @event)  //registra evento
        => _events.Add(@event);

    public void ClearDomainEvents()  //pulisce eventi dopo che sono stati pubblicati da MediatR
        => _events.Clear();
}
