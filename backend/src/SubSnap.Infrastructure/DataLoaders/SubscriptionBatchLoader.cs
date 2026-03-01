using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Persistence.Context;
using System.Collections.Concurrent;

namespace SubSnap.Infrastructure.DataLoaders;

/*
 * NON SONO query parallele(quer1 - query2 ...tutte in parallelo. Db overload!!)! sono CUNCURRENT BATCHED (requests parallele -> 1 sola query ottimizzata! db happy!!), 
 * 
 * ors invece di :  await repo.GetSubscriptions(user.Id);
 * fai (in handler o orchestrator): 
var tasks = users.Select(u =>
    _subscriptionLoader.Load(u.Id, ct));
var subscriptions = await Task.WhenAll(tasks);
 */

public sealed class SubscriptionBatchLoader
    : ISubscriptionBatchLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;
    private readonly ILogger<SubscriptionBatchLoader> _logger;

    private readonly ConcurrentDictionary<Guid,
        TaskCompletionSource<IReadOnlyList<Subscription>>> _pending
            = new();

    private bool _scheduled;
    private readonly object _lock = new();

    public SubscriptionBatchLoader(
        IDbContextFactory<ApplicationDbContext> factory,
        ILogger<SubscriptionBatchLoader> logger
    )
    {
        _factory = factory;
        _logger = logger;
    }
    public Task<IReadOnlyList<Subscription>> Load(
        UserId userId,
        CancellationToken ct = default)
    {
        var key = userId.Value;

        var tcs = _pending.GetOrAdd(
            key,
            _ => new TaskCompletionSource<IReadOnlyList<Subscription>>(
                TaskCreationOptions.RunContinuationsAsynchronously));

        ScheduleExecution();

        return tcs.Task;
    }

    private void ScheduleExecution()
    {
        lock (_lock)
        {
            if (_scheduled) return;
            _scheduled = true;
        }

        _ = Task.Run(ExecuteBatch);
    }

    private async Task ExecuteBatch()
    {
        await Task.Delay(5); // batching window

        var snapshot = _pending.ToArray();
        var ids = snapshot.Select(x => x.Key).ToList();

        await using var db =
            await _factory.CreateDbContextAsync();

        var subs = await db.Set<Subscription>()
            .Where(s => ids.Contains(
                EF.Property<Guid>(s, "UserId")))
            .ToListAsync();

        var grouped = subs
            .GroupBy(s =>
                EF.Property<Guid>(s, "UserId"))
            .ToDictionary(g => g.Key, g =>
                (IReadOnlyList<Subscription>)g.ToList());

        foreach (var (id, tcs) in snapshot)
        {
            grouped.TryGetValue(id, out var result);
            tcs.TrySetResult(result ?? new List<Subscription>());
            _pending.TryRemove(id, out _);
        }

        lock (_lock)
            _scheduled = false;
    }
}