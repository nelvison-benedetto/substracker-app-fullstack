using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.DataLoadersorQueries;

public interface ISubscriptionAggregateLoader
{
    Task<SubscriptionSubscriptionHistoriesAggregate?> LoadWithSubscriptionHistoriesAsync(SubscriptionId subscriptionId, CancellationToken ct = default);
}
