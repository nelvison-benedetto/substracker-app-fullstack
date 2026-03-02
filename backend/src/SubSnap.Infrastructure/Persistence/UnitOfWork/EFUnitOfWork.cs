using MediatR;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Common;
using SubSnap.Infrastructure.Persistence.Context;
using SubSnap.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace SubSnap.Infrastructure.Persistence.UnitOfWork;

public sealed class EFUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;

    public EFUnitOfWork(ApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {

        //old without OUTOFBOX pattern
        //await _context.SaveChangesAsync(ct);
        //var aggregates = _context.ChangeTracker
        //    .Entries<AggregateRoot>()
        //    .Select(e => e.Entity)
        //    .Where(e => e.DomainEvents.Any())
        //    .ToList();
        //var events = aggregates
        //    .SelectMany(a => a.DomainEvents)
        //    .ToList();
        //aggregates.ForEach(a => a.ClearDomainEvents());
        //foreach (var domainEvent in events)
        //{
        //    await _mediator.Publish(domainEvent, ct);
        //}


        //!!OUTBOX PATTERN. e.g. vuoi fare Saved Used + Send Email, quindi devi fare le 2 operazioni insime perche la prima non puo essere DONE senza la seconda, altrimenti hai inconsistenza della transazione! Quindi invece di inviare direttamente l'email, salvi un record in un Outbox table, e poi un processo separato legge da quella tabella e invia le email. In questo modo, anche se l'invio dell'email fallisce, il record rimane nell'Outbox e può essere ritentato.
        //EF tiene traccia di tutte le entità che sono state modificate, quindi puoi facilmente estrarre gli eventi di dominio dalle entità modificate e salvarli nella tabella Outbox insieme alle modifiche dello stato, garantendo così la consistenza della transazione.

        //extract domain events from tracked entities
        var domainEvents = _context.ChangeTracker
            .Entries<AggregateRoot>()  //CHANGETRACKER, dice 'dammi tutte le entità tracciate che derivano da AggregateRoot'
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        //clear domain events
        foreach (var entity in _context.ChangeTracker.Entries<AggregateRoot>())
        {
            entity.Entity.ClearDomainEvents();  //evita doppia pubblicazione degli eventi di dominio, una volta che sono stati estratti e salvati nell'Outbox, li puliamo dalle entità per evitare che vengano pubblicati nuovamente in futuro
        }

        //save domain events to outbox
        foreach (var domainEvent in domainEvents)
        {
            var message = new OutboxMessage  //crei un nuovo obj per ciascun domain event trovato
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().AssemblyQualifiedName!,  //CONVERT domain event -->  OUTBOX ROW type(outboxmessage.cs), in questo modo quando lo leggo posso capire di che tipo di evento si tratta e deserializzarlo correttamente. usi AssemblyQualifiedName non invece .Name (fallirebbe in produzione!!)
                Payload = JsonSerializer.Serialize(
                    domainEvent,
                    domainEvent.GetType()),
                OccurredOnUtc = DateTime.UtcNow
            };
            _context.OutboxMessages.Add(message);  //ADD TO DB
        }
        //SINGLE TRANSACTION COMMIT: save both the state changes and the outbox messages together to ensure consistency!!
        await _context.SaveChangesAsync(ct);  //ORA FAI IL DEFINITIVO COMMIT, salvando persistendo l'user added e il/i outboxmessages added. TRANSAZIONE ATOMICA!!

        /*
         ora hai 
        User insert
        +
        Outbox insert
        =
        ONE TRANSACTION
        quindi se fallisce è un rollerback totale!
         */

    }
}
