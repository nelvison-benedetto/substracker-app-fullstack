using MediatR;
using SubSnap.Application.Ports.Persistence;
using SubSnap.Core.Domain.Common;
using SubSnap.Infrastructure.Persistence.Context;

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

    public async Task SaveChangesAsync(CancellationToken ct = default)
    { 
        await _context.SaveChangesAsync(ct);

        //DOMAIN EVENT management (xk magari quando registri un nuovo user vuoi automaticamente inviare un'email, ma DEVI ASSICURARTI di inviare email DOPO che registrazione su db sia un successo!!consistency garantita)
        //EF tiene traccia di tutte le entità che sono state modificate!!
        //1. Collect aggregates
        var aggregates = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        //2. Extract events
        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        //3. Clear events
        aggregates.ForEach(a => a.ClearDomainEvents());

        //4. Publish
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, ct);
        }

    }
}
