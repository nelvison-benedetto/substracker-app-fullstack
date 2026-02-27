using SubSnap.Core.Domain.ValueObjects;

namespace SubSnap.Core.Domain.Entities;

public class Subscription
{
    public SubscriptionId Id { get; private set; }
    public string Name { get; private set; }
    public decimal Amount { get; private set; }
    public string BillingCycle { get; private set; }
    public DateOnly StartDate { get; }
    public DateOnly? EndDate { get; private set; }
    public string? Category { get; private set; }

    protected Subscription() { }  //constructor!!

    public Subscription(
        //SubscriptionId? id,
        string name,
        decimal amount,
        string billingCycle,
        DateOnly startDate,
        DateOnly? endDate,
        string? category)
    {
        Id = SubscriptionId.New();
        Name = name;
        Amount = amount;
        BillingCycle = billingCycle;
        StartDate = startDate;
        EndDate = endDate;
        Category = category;
    }
}
