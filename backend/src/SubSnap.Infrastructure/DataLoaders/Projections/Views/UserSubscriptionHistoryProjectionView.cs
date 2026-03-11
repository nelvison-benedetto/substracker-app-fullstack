using SubSnap.Core.Domain.Entities;

namespace SubSnap.Infrastructure.DataLoaders.Projections.Views;

/*
non va nel Domain, xk è un read models delle query!! non è un business aggregate! non rappresenta uno STATO reale di dominio!!
 */
public sealed class UserSubscriptionHistoryProjectionView
{
    public User User { get; }

    public Subscription Subscription { get; }

    public IReadOnlyCollection<SubscriptionHistory> History { get; }

    public UserSubscriptionHistoryProjectionView(
        User user,
        Subscription subscription,
        IReadOnlyCollection<SubscriptionHistory> history)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        History = history?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(history));
    }

}
