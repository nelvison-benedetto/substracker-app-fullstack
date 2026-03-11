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
public sealed class UserAggregateLoader : IUserAggregateLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory; //x .WhenAll() cioe query in parallelo

    public UserAggregateLoader(
        IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserFullAggregate?> LoadWithFullAsync(UserId userId, CancellationToken ct = default)
    {
        await using var userContext = await _factory.CreateDbContextAsync(ct);
        await using var refreshContext = await _factory.CreateDbContextAsync(ct);
        await using var linkContext = await _factory.CreateDbContextAsync(ct);

        var userTask = userContext.Users  //senza 'await' here
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        //parallel loads dei childrens target
        var refreshTokensTask = refreshContext.Set<RefreshToken>()
            .AsNoTracking()
            .Where(rt => EF.Property<Guid>(rt, "UserId") == userId.Value)
            .ToListAsync(ct);

        var sharedLinksTask = linkContext.Set<SharedLink>()
            .AsNoTracking()
            .Where(sl => EF.Property<Guid>(sl, "UserId") == userId.Value)
            .ToListAsync(ct);

        await Task.WhenAll(userTask, refreshTokensTask, sharedLinksTask);  //run queries in parallelo!!
           //Thread A → context #1 → SELECT users ...
           //Thread B → context #2 → SELECT refreshtokens...
           //Thread C → context #3 → SELECT sharedlinks...
           //db lavora meglio perche ha query piccole su threads diversi(in questo caso il db riceve 2 threads e li runna CONTEMPORANEAMENTE (PARALLELISMO)), non 1 mega join!!

        if (userTask.Result == null)
            return null;

        return new UserFullAggregate(
            userTask.Result,
            refreshTokensTask.Result,
            sharedLinksTask.Result
        );  //costruzione aggregate, e lo restituisco!(questo obj non esiste nel db, è un runtime business projection)
    }

    public async Task<UserSharedLinksAggregate?> LoadWithSharedLinksAsync(UserId userId, CancellationToken ct = default)
    {
        await using var userContext = await _factory.CreateDbContextAsync(ct);
        await using var linkContext = await _factory.CreateDbContextAsync(ct);

        var userTask = userContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        var sharedLinksTask = linkContext.Set<SharedLink>()
            .AsNoTracking()
            .Where(sl => EF.Property<Guid>(sl, "UserId") == userId.Value)
            .ToListAsync(ct);

        await Task.WhenAll( userTask, sharedLinksTask );

        if (userTask.Result == null)
            return null;

        return new UserSharedLinksAggregate(
            userTask.Result,
            sharedLinksTask.Result
        );
    }



    //wrong!! 2 agggregate roots!! TO DELETE!!
    public async Task<UserSubscriptionsAggregate?> LoadWithSubscriptionsAsync( UserId userId, CancellationToken ct = default)
    {
        await using var context = await _factory.CreateDbContextAsync(ct); 
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return null;
        //parallel load dei childrens target
        var subscriptionsTask = LoadSubscriptions(userId, ct); //return la lista di Subscription!
        await Task.WhenAll( subscriptionsTask);  //run queries in parallelo!!
        return new UserSubscriptionsAggregate( user, subscriptionsTask.Result);
    }
    //---- Helpers ----
    private async Task<List<Subscription>> LoadSubscriptions( UserId userId, CancellationToken ct)
    {
        await using var context =
            await _factory.CreateDbContextAsync(ct);

        return await context.Set<Subscription>()
            .AsNoTracking()
            .Where(s =>
                EF.Property<Guid>(s, "UserId") == userId.Value)
            .ToListAsync(ct);
    }


    
}
