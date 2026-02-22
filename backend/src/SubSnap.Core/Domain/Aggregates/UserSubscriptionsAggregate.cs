using SubSnap.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.Domain.Aggregates;

//DOMAIN-FIRST approach
//aggregate esplicito, caricato solo quando serve!
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
    //se vuoi solo e.g. user+ subscription: nel repo usi method GetUserWithSubscriptionsAsync() che return type <UserSubscriptionsAggregate>
}
