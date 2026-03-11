using SubSnap.Application.Ports.DataLoadersorQueries;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Infrastructure.DataLoaders.Projections;

//ogni xxxProjection.cs solitamente è solo x 1 solo caso d'uso. e.g.LoadDashboard() / LoadStats() / ect..
//here combinazione 1 Aggregate Root + 1aggregate root solo perche serve recuperare un suo children!
public class UserSubscriptionHistoryProjectionLoader
{
    private readonly IUserAggregateLoader _userLoader;
    private readonly ISubscriptionAggregateLoader _subscriptionLoader;

    public UserSubscriptionHistoryProjectionLoader(
        IUserAggregateLoader userLoader,
        ISubscriptionAggregateLoader subscriptionLoader)
    {
        _userLoader = userLoader;
        _subscriptionLoader = subscriptionLoader;
    }

    public async Task<UserSubscriptionHistoryProjectionView?> LoadAsync(
        UserId userId,
        SubscriptionId subscriptionId,
        CancellationToken ct = default)
    {
        var userTask = _userLoader.LoadWithFullAsync(userId, ct);

        var subscriptionTask =
            _subscriptionLoader.LoadWithSubscriptionHistoriesAsync(subscriptionId, ct);

        await Task.WhenAll(userTask, subscriptionTask);

        if (userTask.Result == null || subscriptionTask.Result == null)
            return null;

        return new UserSubscriptionHistoryProjectionView(
            userTask.Result,
            subscriptionTask.Result);
    }

}
