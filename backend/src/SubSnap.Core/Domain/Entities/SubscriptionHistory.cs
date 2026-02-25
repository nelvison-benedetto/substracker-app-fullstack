namespace SubSnap.Core.Domain.Entities;

public class SubscriptionHistory
{
    //non ha Id, è Value-like. vive solo dentro Subscription.
    public string Action { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }
    public DateTime CreatedAt { get; }

    public SubscriptionHistory(
        string action,
        string? oldValue,
        string? newValue,
        DateTime createdAt)
    {
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = createdAt;
    }
}