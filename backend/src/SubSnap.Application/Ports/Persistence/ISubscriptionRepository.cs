using SubSnap.Core.Domain.Aggregates;
using SubSnap.Core.Domain.Entities;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Application.Ports.Persistence;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(SubscriptionId id);
    Task<SubscriptionAggregate?> GetAggregateAsync(SubscriptionId id);
    Task AddAsync(Subscription subscription);
}
