using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Entities;

public class SubscriptionHistory
{
    public SubscriptionHistoryId Id { get; private set; }
    public string Action { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }

    public DateTime CreatedAt { get; }

    protected SubscriptionHistory() { }

    public SubscriptionHistory(
        string action,
        string? oldValue,
        string? newValue,
        DateTime createdAt)
    {
        Id = SubscriptionHistoryId.New();
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = DateTime.UtcNow;
    }
}