using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.DataLoadersorQueries;

public interface ISubscriptionAggregateLoader
{
    Task<SubscriptionSubscriptionHistoriesAggregate> LoadWithSubscriptionHistories(SubscriptionId subscriptionId, CancellationToken ct);
}
