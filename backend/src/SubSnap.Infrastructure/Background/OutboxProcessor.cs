using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubSnap.Infrastructure.Persistence.Context;
using System.Text.Json;

namespace SubSnap.Infrastructure.Background;

/*
 * OUTBOX pattern
 senza puo accadere nel flow
Save User ✅
Publish Event → Send Email ❌ crash

con outbox invece
Transaction:
   Save User
   Save OutboxMessage
COMMIT ✅

Background Worker: 
   reads Outbox
   publishes event
//se email fallisce , il record rimane nell'outbox e può essere ritentato, senza rischiare inconsistenza della transazione
 */
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMediator _mediator;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IMediator mediator)
    {
        _scopeFactory = scopeFactory;
        _mediator = mediator;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)  //LOOP INFINITO che continua a leggere i messaggi dall'outbox finché il servizio è attivo (non viene shutdown dall'user)
        {
            using var scope = _scopeFactory.CreateScope();  //crei scope manuale, xk backgrounservice è singleton, e applicationdbcontext è scoped, quindi devi creare scope manuale per risolverlo

            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>(); //nuovo dbcontext per ogni ciclo, così eviti problemi di concorrenza e garantisci che ogni ciclo lavori con un contesto pulito

            var messages = await db.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null)
                .Take(20)
                .OrderBy(x => x.OccurredOnUtc)
                .ToListAsync(stoppingToken);
            //lettura outbox, prendi i primi 20 messaggi non processati (processedOnUtc == null), così eviti di sovraccaricare il sistema se ci sono molti messaggi da processare

            foreach (var msg in messages)
            {
                var type = Type.GetType(msg.Type)!;  //!!xk devi salvare AssemblyQualifiedName del tipo, così puoi deserializzare correttamente l'evento di dominio, anche se hai più eventi con lo stesso nome in assembly diversi. non .Name xk altrimenti in produzione fallirà!

                var domainEvent =
                    JsonSerializer.Deserialize(
                        msg.Payload, type);
                //deserializzazione evento di dominio, così ricostruisci l'evento di dominio originale a partire dal payload JSON salvato nell'outbox

                await _mediator.Publish(
                    (INotification)domainEvent!,
                    stoppingToken);
                //publish evento di dominio usando MediatR, ora MediatR chiama UserRegisteredHandler

                msg.ProcessedOnUtc = DateTime.UtcNow;  //lo marchi (see OutboxMessage.cs)
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(2000, stoppingToken);  //!!POLLING OGNI 2 SEC
        }
    }
}
