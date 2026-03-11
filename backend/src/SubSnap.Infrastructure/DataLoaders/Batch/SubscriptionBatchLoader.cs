using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Persistence.Context;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SubSnap.Infrastructure.DataLoaders.Batch;

/*
 * ottimizza query READS multiple simultanee: N richieste -> 1 query!
 * 
 * NON SONO query parallele(quer1 - query2 ...tutte in parallelo. db overload!!)! sono CUNCURRENT BATCHED (requests parallele -> 1 sola query ottimizzata! db happy!!), 
 * 
 * per utilizzarlo fai 
var tasks = users.Select(u =>
    _subscriptionLoader.Load(u.Id, ct));
var subscriptions = await Task.WhenAll(tasks);

viene chiamato dall'handler.
Handler / Orchestrator
       ↓
SubscriptionBatchLoader.Load(userId)
       ↓
pending dictionary
       ↓ (5ms)
ExecuteBatch()
       ↓
SELECT ... WHERE UserId IN (...)

see  getuserswithsubscriptionshandler.cs  useraggregateloader.cs  subscriptionbatchloader.cs  userrepository.cs  GUSHandler.cs(top!)
 */

public sealed class SubscriptionBatchLoader : ISubscriptionBatchLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly ILogger<SubscriptionBatchLoader> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;  //x leggere correlationId dal middlewre corrente

    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<IReadOnlyList<Subscription>>> _pending = new();
    //TaskCompletionSource è un promise manuale, tu prometti 'ti darò il risultato piu tardi'.

    private bool _scheduled;
    private readonly object _lock = new();

    public SubscriptionBatchLoader(
        IDbContextFactory<ApplicationDbContext> factory,
        ILogger<SubscriptionBatchLoader> logger,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _factory = factory;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IReadOnlyList<Subscription>> Load(
        UserId userId,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Batch request queued for UserId {UserId}",
            userId.Value);
        //logging ok.

        var key = userId.Value; //serve per raggruppare le req duplicate

        var tcs = _pending.GetOrAdd( key, _ => new TaskCompletionSource<IReadOnlyList<Subscription>>(TaskCreationOptions.RunContinuationsAsynchronously));
        //dice 'se già qualcuno ha chiesto questo user → riusa promessa, altrimenti creala.'

        ScheduleExecution(ct);  //serve ad evitare 100 req -> 100 query

        return tcs.Task;
    }

    private void ScheduleExecution(CancellationToken ct)
    {
        lock (_lock)
        {
            if (_scheduled) return;
            _scheduled = true;
        }
        //garantisce solo 1 batch running attivo!!

        _ = Task.Run(ExecuteBatch(ct));  //parte in background
    }
    
    private async Task ExecuteBatch(CancellationToken ct)
    {
        await Task.Delay(5); //aspetti 5ms, perche serve un po di tempo x raccogliere le req simultanee. e.g. t=0ms user1 t=1ms user2 t=3ms user3...

        var snapshot = _pending.ToArray(); //congeli le req attuali
        var ids = snapshot.Select(x => x.Key).ToList();  //ora hai e.g. [user1, user2, user3]

        await using var db =
            await _factory.CreateDbContextAsync(ct);

        //OLD before logging x batch loader
        //var subs = await db.Set<Subscription>()
        //    .Where(s => ids.Contains(
        //        EF.Property<Guid>(s, "UserId")))
        //    .ToListAsync();
        //var grouped = subs
        //    .GroupBy(s =>
        //        EF.Property<Guid>(s, "UserId"))
        //    .ToDictionary(g => g.Key, g =>
        //        (IReadOnlyList<Subscription>)g.ToList());
        //foreach (var (id, tcs) in snapshot)
        //{
        //    grouped.TryGetValue(id, out var result);
        //    tcs.TrySetResult(result ?? new List<Subscription>());
        //    _pending.TryRemove(id, out _);
        //}
        //lock (_lock)
        //    _scheduled = false;

        //LOGGING SPECIFICO anche X BATCH LOADERS
        // Recupero CorrelationId dal contesto HTTP
        var correlationId = _httpContextAccessor.HttpContext?.Items["CorrelationId"]
            ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        using (_logger.BeginScope( new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Batch"] = "SubscriptionLoader"
        })) //aggiunge auto CorrelationId Batch=SubscriptionLoader a tutti i logs all'interno
        {
            _logger.LogInformation(
                "Executing subscription batch query for {Count} users",
                ids.Count);

            var sw = Stopwatch.StartNew();  //x performace monitoring in tempo reale. see performancebehvior.cs

            var subs = await db.Set<Subscription>()
                .Where(s => ids.Contains(EF.Property<Guid>(s, "UserId")))
                .ToListAsync();

            sw.Stop();

            _logger.LogInformation(
                "Batch query completed in {ElapsedMs}ms returning {Rows} rows",
                sw.ElapsedMilliseconds,
                subs.Count);

            var grouped = subs
                .GroupBy(s => EF.Property<Guid>(s, "UserId"))
                .ToDictionary(g => g.Key, g => (IReadOnlyList<Subscription>)g.ToList());

            foreach (var (id, tcs) in snapshot)
            {
                grouped.TryGetValue(id, out var result);
                tcs.TrySetResult(result ?? new List<Subscription>());
                _pending.TryRemove(id, out _);
            }
        }

        lock (_lock)
            _scheduled = false; //reset, cosi permette prossimo batch

    }
}