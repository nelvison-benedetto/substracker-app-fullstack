namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct Email  //è readonly struct
{
    public string Value { get; }
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required");

        if (!value.Contains('@'))
            throw new ArgumentException("Invalid email");

        Value = value;
    }

    public override string ToString() => Value;

    //equals
    public bool Equals(Email other) => Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public static bool operator == (Email left, Email right) => left.Equals(right);
    public static bool operator != (Email left, Email right) => !(left == right);
}
