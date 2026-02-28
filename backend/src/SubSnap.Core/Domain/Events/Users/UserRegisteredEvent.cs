using SubSnap.Core.Domain.Common;
using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Events.Users;

/*
 */
public sealed class UserRegisteredEvent : IDomainEvent
{
    public UserId UserId { get; }

    public DateTime OccurredOnUtc { get; }
        = DateTime.UtcNow;

    public UserRegisteredEvent(UserId userId)
    {
        UserId = userId;
    }
}
