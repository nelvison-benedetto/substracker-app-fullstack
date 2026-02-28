using MediatR;

namespace SubSnap.Core.Domain.Common;

public interface IDomainEvent : INotification  //plugin MediatR, cosi ogni volta che viene creato un DomainEvent, questo viene automaticamente pubblicato (pub/sub) da MediatR, e tutti i DomainEventHandler che lo gestiscono vengono eseguiti.
{
    DateTime OccurredOnUtc { get; }
}
