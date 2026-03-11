using SubSnap.Core.Domain.Entities;

namespace SubSnap.Core.Domain.Aggregates;

public class SubscriptionSubscriptionHistoriesAggregate
{
    public Subscription Subscription { get; }
    public IReadOnlyCollection<SubscriptionHistory> SubscriptionHistories { get; }

    public SubscriptionSubscriptionHistoriesAggregate(
        Subscription subscription,
        IReadOnlyCollection<SubscriptionHistory> subscriptionHistories
    )
    {
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        SubscriptionHistories = subscriptionHistories?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(subscriptionHistories));
    }

}
