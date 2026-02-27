namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct SubscriptionId
{
    public Guid Value { get; }
    public SubscriptionId(Guid value)
    {
        if (value == Guid.Empty) 
            throw new ArgumentException("SubscriptionId cannot be empty GUID");
        Value = value;
    }
    public static SubscriptionId New() => new SubscriptionId(Guid.NewGuid());  //generi nuovo Id!! COSI IL DOMAIN E' INDIPENDENTE DAL DB, e non devi fare un SaveChanges() solo per ottenere l'id delle nuova row!!

    public override string ToString() => Value.ToString();

    //equals
    public bool Equals(UserId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is SubscriptionId other && Equals(other);
    public static bool operator == (SubscriptionId left, SubscriptionId right) => left.Equals(right);
    public static bool operator != (SubscriptionId left, SubscriptionId right) => !(left == right);

}
