using SubSnap.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.Domain.Aggregates;

//x DOMAIN-FIRST approach
//questo è il 'VERO' User completo(w list Subscriptions e list SharedLinks). NON sempre caricato, solo quando serve
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
        Subscriptions = subscriptions?.ToList().AsReadOnly() //Espone ReadOnly (sicuro)
            ?? throw new ArgumentNullException(nameof(subscriptions));
        SharedLinks = sharedLinks?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(sharedLinks));
    }
    
}
/*
esempio di come fai ad ottenere i dati aggregati correttamente
//cmnq caricare user+tutti i suoi dati collegati si usa il 20% delle volte, la maggiorparte delle volte ti serve solo ottenere l'User senza i suoi dati collegati.

//OLD APPROACH
 nel repository
public async Task<UserAggregate?> GetAggregateAsync(UserId id)
{
    var entity = await _context.User
        .Include(u => u.Subscription)
        .Include(u => u.SharedLink)
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.UserId == id.Value);

    return entity is null
        ? null
        : UserAggregateMapper.ToDomain(entity);
}
nel service
public async Task<UserDashboardResult> GetDashboardAsync(UserId userId)
{
    var aggregate = await _userRepository.GetAggregateAsync(userId);

    if (aggregate is null)
        throw new UserNotFoundException(userId.Value);

    // ora hai tutto il grafo coerente
    return new UserDashboardResult(
        aggregate.User.Email.Value,
        aggregate.Subscriptions.Count,
        aggregate.Subscriptions.Sum(s => s.Amount)
    );
}
 */