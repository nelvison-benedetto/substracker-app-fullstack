using SubSnap.Core.Domain.Entities;

namespace SubSnap.Core.Domain.Aggregates;

public class UserSharedLinksAggregate
{
    public User User { get; }
    public IReadOnlyCollection<SharedLink> SharedLinks { get; }

    public UserSharedLinksAggregate(
        User user,
        IReadOnlyCollection<SharedLink> sharedLinks
    )
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        SharedLinks = sharedLinks?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(sharedLinks));
    }
}
