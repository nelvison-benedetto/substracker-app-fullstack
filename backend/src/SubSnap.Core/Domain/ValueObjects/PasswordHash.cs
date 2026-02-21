using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSnap.Core.Domain.ValueObjects;

public readonly struct PasswordHash  //è readonly struct
{
    public string Value { get; }
    public PasswordHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PasswordHash is required");

        Value = value;
    }
    public override string ToString() => Value.ToString();
}
