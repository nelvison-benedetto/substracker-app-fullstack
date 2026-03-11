using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.DataLoaders.Projections;

//combinazione 2 Aggregate Roots!!
public class UserSubscriptionsProjectionLoader
{
    private readonly IUserAggregateLoader _userLoader;
    private readonly ISubscriptionBatchLoader _subscriptionLoader;

    public UserSubscriptionsProjectionLoader(
        IUserAggregateLoader userLoader,
        ISubscriptionBatchLoader subscriptionLoader)
    {
        _userLoader = userLoader;
        _subscriptionLoader = subscriptionLoader;
    }

    public async Task<UserSubscriptionsProjection?> Load(
        UserId userId,
        CancellationToken ct = default)
    {
        var userTask = _userLoader.LoadWithFullAsync(userId, ct);
        var subsTask = _subscriptionLoader.Load(userId, ct);

        await Task.WhenAll(userTask, subsTask);

        if (userTask.Result == null)
            return null;

        return new UserSubscriptionsProjection(
            userTask.Result,
            subsTask.Result);
    }


}
