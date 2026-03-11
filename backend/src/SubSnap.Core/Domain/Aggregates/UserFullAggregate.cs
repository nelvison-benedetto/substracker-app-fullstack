using SubSnap.Core.Domain.Entities;

namespace SubSnap.Core.Domain.Aggregates;

public class UserFullAggregate
{
    public User User { get; }
    public IReadOnlyCollection<RefreshToken> RefreshTokens { get; }
    public IReadOnlyCollection<SharedLink> SharedLinks { get; }

    public UserFullAggregate(
        User user,
        IReadOnlyCollection<RefreshToken> refreshTokens,
        IReadOnlyCollection<SharedLink> sharedLinks
    )
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
        RefreshTokens = refreshTokens?.ToList().AsReadOnly()  //Espone ReadOnly (sicuro)
            ?? throw new ArgumentNullException(nameof(refreshTokens));
        SharedLinks = sharedLinks?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(sharedLinks));
    }

}
