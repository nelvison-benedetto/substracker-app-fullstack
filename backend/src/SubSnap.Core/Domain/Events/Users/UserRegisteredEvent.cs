using SubSnap.Core.Domain.Common;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Events.Users;

/*
 * l'evento di dominio UserRegisteredEvent rappresenta un evento che si verifica quando un nuovo utente si registra nel sistema. Contiene informazioni rilevanti sull'evento, come l'ID dell'utente e la data e ora in cui l'evento è avvenuto. Questo evento può essere utilizzato per attivare azioni specifiche, come l'invio di email di benvenuto, l'aggiornamento delle statistiche degli utenti o l'integrazione con sistemi esterni.
 */
public sealed class UserRegisteredEvent : IDomainEvent  //implementa IDomainEvent xk è un domain event, e quindi verra automaticamente pubblicato da MediatR ogni volta che viene creato, e tutti i DomainEventHandler che lo gestiscono vengono eseguiti (e.g. SendWelcomeEmailHandler, UpdateUserStatsHandler, etc).
{
    public UserId UserId { get; }

    public DateTime OccurredOnUtc { get; }
        = DateTime.UtcNow;

    public UserRegisteredEvent(UserId userId)
    {
        UserId = userId;
    }
}
