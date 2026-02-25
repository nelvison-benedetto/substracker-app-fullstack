using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.DataLoadersorQueries;

public interface IUserAggregateLoader
{
    Task<UserSubscriptionsAggregate?> LoadWithSubscriptions(UserId userId, CancellationToken ct = default);
    //Task<List<Subscription>> LoadSubscriptions(UserId userId, CancellationToken ct);

}
