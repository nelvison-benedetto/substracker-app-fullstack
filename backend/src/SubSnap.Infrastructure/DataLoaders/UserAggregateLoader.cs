using Microsoft.EntityFrameworkCore;
using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using SubSnap.Infrastructure.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Infrastructure.DataLoaders;

public class UserAggregateLoader
{
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public UserAggregateLoader(
        IDbContextFactory<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<UserSubscriptionsAggregate?> LoadWithSubscriptions(
        UserId userId,
        CancellationToken ct = default)
    {
        // context #1 → user
        await using var userContext =
            await _factory.CreateDbContextAsync(ct);

        var user = await userContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return null;

        // PARALLEL CONTEXTS
        var subscriptionsTask = LoadSubscriptions(userId, ct);

        await Task.WhenAll(subscriptionsTask);

        return new UserSubscriptionsAggregate(
            user,
            subscriptionsTask.Result);
    }

    private async Task<List<Subscription>> LoadSubscriptions(
        UserId userId,
        CancellationToken ct)
    {
        await using var context =
            await _factory.CreateDbContextAsync(ct);

        return await context.Subscriptions
            .AsNoTracking()
            .Where(s =>
                EF.Property<Guid>(s, "UserId") == userId.Value)
            .ToListAsync(ct);
    }
    //Thread A → DbContext #1 → Users
    //Thread B → DbContext #2 → Subscriptions
}
