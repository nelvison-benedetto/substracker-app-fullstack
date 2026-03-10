using SubSnap.Core.Domain.Entities;
using SubSnap.Infrastructure.Persistence.Context;

namespace SubSnap.Infrastructure.Repositories.Implementations;

public class SubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public SubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Subscription subscription, CancellationToken ct)
    {
        _context.Subscriptions.Add(subscription);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(Subscription subscription, CancellationToken ct)
    {
        _context.Subscriptions.Remove(subscription);
        return Task.CompletedTask;
    }

}
