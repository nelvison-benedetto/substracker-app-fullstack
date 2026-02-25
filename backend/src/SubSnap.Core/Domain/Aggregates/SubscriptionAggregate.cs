using SubSnap.Core.Domain.Entities;

namespace SubSnap.Core.Domain.Aggregates;

// Subscription + History = confine di consistenza
public sealed class SubscriptionAggregate
{
    public Subscription Subscription { get; }
    public IReadOnlyCollection<SubscriptionHistory> History { get; }

    public SubscriptionAggregate(
        Subscription subscription,
        IEnumerable<SubscriptionHistory> history)
    {
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        History = history?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(history));
    }
    //SubscriptionHistory vive solo qui, non da sola!
}
