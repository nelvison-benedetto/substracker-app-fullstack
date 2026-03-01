using MediatR;

namespace SubSnap.Core.Domain.Common;

/*
 * qui colleghi DomainEvent --> plugin MediatR, in questo modo ogni volta che viene creato un DomainEvent, questo viene automaticamente pubblicato (pub/sub) da MediatR, e tutti i DomainEventHandler che lo gestiscono vengono eseguiti.
   IDomainEvent verra poi usato in tutti gli events, e.g. UserRegisteredEvent : IDomainEvent, così ogni volta che viene creato un UserRegisteredEvent, questo viene automaticamente pubblicato da MediatR, e tutti i DomainEventHandler che lo gestiscono vengono eseguiti (e.g. SendWelcomeEmailHandler, UpdateUserStatsHandler, etc).
 */
public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
