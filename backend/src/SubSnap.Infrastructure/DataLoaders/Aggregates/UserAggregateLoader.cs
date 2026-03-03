using Microsoft.EntityFrameworkCore;
using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Persistence.Context;

namespace SubSnap.Infrastructure.DataLoaders.Aggregates;

/*
 * 
 */
public sealed class UserAggregateLoader : IUserAggregateLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory; //x .WhenAll() cioe query in parallelo.

    public UserAggregateLoader(
        IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserSubscriptionsAggregate?> LoadWithSubscriptions( UserId userId, CancellationToken ct = default)
    {
        // context #1 → user
        await using var userContext =
            await _factory.CreateDbContextAsync(ct); //x nuovo db context isolato

        var user = await userContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        // PARALLEL CONTEXTS
        var subscriptionsTask = LoadSubscriptions(userId, ct); //ritorna la lista di Subscription

        await Task.WhenAll(subscriptionsTask);  //run queries in parallelo!!
            //Thread A → DbContext #1 → Users
            //Thread B → DbContext #2 → Subscriptions
            //db lavora meglio perche ha query piccole su threads diversi, non 1 mega join!!

        return new UserSubscriptionsAggregate( user, subscriptionsTask.Result); //costruzione aggregate, e lo restituisco!(questo obj non esiste nel db, è un runtime business projection)
    }

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
