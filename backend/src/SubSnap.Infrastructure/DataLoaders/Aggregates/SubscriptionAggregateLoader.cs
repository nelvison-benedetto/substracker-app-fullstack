using Microsoft.EntityFrameworkCore;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Persistence.Context;

namespace SubSnap.Infrastructure.DataLoaders.Aggregates;

/*
 * PROIEZIONE RUNTIME, load aggregate root (e.g.user) + combinations di CHILDRENS(quindi no altri aggregates roots!e.g.subscriptions) ...
 * assolutamente NON USARE .Include() (mega-join mostruosa)!!
 * 
 * xxxAggregateLoader non conosce i BATCHLOADERS!! è solo un builder.
 * viene chiamato dall' handler/orchestrator.
 * 
see  getuserswithsubscriptionshandler.cs  useraggregateloader.cs  subscriptionbatchloader.cs  userrepository.cs  GUSHandler.cs(top!)
 */
// usa AutoMapper nel .API per e.g.UserAggregate → UserDashboardDTO e passare questo al client.
public sealed class SubscriptionAggregateLoader : ISubscriptionAggregateLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory; //x .WhenAll() cioe query in parallelo

    public SubscriptionAggregateLoader(IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<SubscriptionSubscriptionHistoriesAggregate?> LoadWithSubscriptionHistoriesAsync(SubscriptionId subscriptionId, CancellationToken ct = default)
    {
        await using var subscriptionsContext = await _factory.CreateDbContextAsync(ct);
        await using var subscriptionHistoriesContext = await _factory.CreateDbContextAsync(ct);

        var subscriptionTask = subscriptionsContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == subscriptionId, ct);

        var subscriptionHistoriesTask = subscriptionHistoriesContext.Set<SubscriptionHistory>()
            .AsNoTracking()
            .Where(sh => EF.Property<Guid>(sh, "SubscriptionId") == subscriptionId.Value)
            .ToListAsync(ct);

        await Task.WhenAll( subscriptionTask, subscriptionHistoriesTask );

        if (subscriptionTask.Result == null)
            return null;

        return new SubscriptionSubscriptionHistoriesAggregate(
            subscriptionTask.Result,
            subscriptionHistoriesTask.Result
        );
    }

}
