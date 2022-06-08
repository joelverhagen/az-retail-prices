namespace Knapcode.AzureRetailPrices.NaturalKeys;

public class DynamicTuple : IEquatable<DynamicTuple>
{
    public DynamicTuple(object?[] values)
    {
        Values = values;

        var hashCode = new HashCode();
        foreach (var value in values)
        {
            hashCode.Add(value);
        }

        _hashCode = hashCode.ToHashCode();
    }

    public IReadOnlyList<object?> Values { get; }

    private readonly int _hashCode;

    public override bool Equals(object? obj)
    {
        return obj is DynamicTuple tuple && Equals(tuple);
    }

    public bool Equals(DynamicTuple? other)
    {
        return other is not null && Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode() => _hashCode;
}
