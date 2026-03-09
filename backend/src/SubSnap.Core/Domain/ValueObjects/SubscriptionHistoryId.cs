namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct SubscriptionHistoryId
{
    public Guid Value { get; }
    public SubscriptionHistoryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SubscriptionId cannot be empty GUID");
        Value = value;
    }
    public static SubscriptionHistoryId New() => new SubscriptionHistoryId(Guid.NewGuid());  //generi nuovo Id!! COSI IL DOMAIN E' INDIPENDENTE DAL DB, e non devi fare un SaveChanges() solo per ottenere l'id delle nuova row!!

    public override string ToString() => Value.ToString();

    //equals
    public bool Equals(SubscriptionHistoryId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is SubscriptionHistoryId other && Equals(other);
    public static bool operator ==(SubscriptionHistoryId left, SubscriptionHistoryId right) => left.Equals(right);
    public static bool operator !=(SubscriptionHistoryId left, SubscriptionHistoryId right) => !(left == right);

    public override int GetHashCode() => Value.GetHashCode(); //x warning CS0659, xk quando fai override di Equal è consigliato farlo anche x GetHashCode()
}
