namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct UserId  //è readonly struct
{
    public Guid Value { get; }  //better GUID di semplice int!!
    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty GUID");
        Value = value;
    }
    public static UserId New() => new UserId(Guid.NewGuid());  //generi nuovo Id!! COSI IL DOMAIN E' INDIPENDENTE DAL DB, e non devi fare un SaveChanges() solo per ottenere l'id delle nuova row!!

    public override string ToString() => Value.ToString();

    //equals
    public bool Equals(UserId other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is UserId other && Equals(other);
    public static bool operator ==(UserId left, UserId right) => left.Equals(right);
    public static bool operator != (UserId left, UserId right) => !(left == right);

}
