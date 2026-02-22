using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Infrastructure.Mapping;

//x DOMAIN-FIRST approach
public sealed class UserAggregate
{
    public User User { get; }
    public IReadOnlyCollection<Subscription> Subscriptions { get; }
    public IReadOnlyCollection<SharedLink> SharedLinks { get; }
    public UserAggregate(
        User user,
        IEnumerable<Subscription> subscriptions,
        IEnumerable<SharedLink> sharedLinks)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        Subscriptions = subscriptions?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(subscriptions));
        SharedLinks = sharedLinks?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(sharedLinks));
    }
}


//OLD APPROACH
//public static class SubscriptionMapper
//{
//    // DB -> Domain
//    public static Subscription ToDomain(Persistence.Scaffold.Subscription entity)
//        => new(
//            new(entity.SubscriptionId),
//            entity.Name,
//            entity.Amount,
//            entity.BillingCycle,
//            entity.StartDate,
//            entity.EndDate,
//            entity.Category
//        );

//    // Domain -> DB
//    public static Persistence.Scaffold.Subscription ToEntity(Subscription domain, int userId)
//        => new()
//        {
//            SubscriptionId = domain.Id.Value,
//            UserId = userId, // 🔑 passato dal repository / aggregate
//            Name = domain.Name,
//            Amount = domain.Amount,
//            BillingCycle = domain.BillingCycle,
//            StartDate = domain.StartDate,
//            EndDate = domain.EndDate,
//            Category = domain.Category
//        };
//}
