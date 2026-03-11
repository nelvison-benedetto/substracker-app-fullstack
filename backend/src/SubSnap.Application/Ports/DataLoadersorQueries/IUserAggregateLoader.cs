using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.DataLoadersorQueries;

public interface IUserAggregateLoader
{
    Task<UserFullAggregate?> LoadWithFullAsync(UserId userId, CancellationToken ct = default);
    Task<UserSharedLinksAggregate?> LoadWithSharedLinksAsync(UserId userId, CancellationToken ct = default);


    //wrong!! 2 aggregate roots
    //Task<UserSubscriptionsAggregate?> LoadWithSubscriptions(UserId userId, CancellationToken ct);

}
