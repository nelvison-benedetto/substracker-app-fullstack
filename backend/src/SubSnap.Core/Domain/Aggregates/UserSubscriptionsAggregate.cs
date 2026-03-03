using SubSnap.Core.Domain.Entities;

namespace SubSnap.Core.Domain.Aggregates;

//user + subscriptions composizione a runtime (non esiste sul db). aggregate esplicito, caricato solo quando serve!
public class UserSubscriptionsAggregate
{
    public User User { get; }
    public IReadOnlyCollection<Subscription> Subscriptions { get; }

    public UserSubscriptionsAggregate(
        User user,
        IReadOnlyCollection<Subscription> subscriptions)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Subscriptions = subscriptions?.ToList().AsReadOnly() //Espone ReadOnly (sicuro)
            ?? throw new ArgumentNullException(nameof(subscriptions));
    }
    //se vuoi solo e.g. user+subscription: nel .infrastructure/dataloaders/UserAggregateLoader.cs usi method LoadWithSubscriptions() che return type <UserSubscriptionsAggregate>
}
