namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct SharedLinkId
{
    public Guid Value { get; }
    public SharedLinkId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("Invalid SharedLinkId");
        Value = value;
    }
}
