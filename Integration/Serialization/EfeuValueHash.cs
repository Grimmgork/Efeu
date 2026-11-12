using System;
using System.Linq;

namespace Efeu.Integration.Utils.Serialization;

public readonly struct EfeuValueHash : IEquatable<EfeuValueHash>
{
    private readonly byte[]? _data;
    private readonly int _hashCode;

    public EfeuValueHash(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        if (data.Length == 0)
        {
            _data = null;
            _hashCode = 0;
            return;
        }
        
        _data = data;
        HashCode hash = new HashCode();
        foreach (byte b in _data)
            hash.Add(b);

        _hashCode = hash.ToHashCode();
    }
    
    public static EfeuValueHash Empty => default;
    
    public int Length => _data?.Length ?? 0;
    
    public bool IsEmpty => Length == 0;
    
    public ReadOnlySpan<byte> Span => _data ?? Span<byte>.Empty;
    
    public bool Equals(EfeuValueHash other)
        => Span.SequenceEqual(other.Span);

    public override bool Equals(object? obj)
        => obj is EfeuValueHash other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    public override string ToString()
        => _data == null ? string.Empty : Convert.ToHexString(_data);

    public static EfeuValueHash Parse(string value)
        => new(Convert.FromHexString(value));

    public static bool operator ==(EfeuValueHash left, EfeuValueHash right)
        => left.Equals(right);

    public static bool operator !=(EfeuValueHash left, EfeuValueHash right)
        => !left.Equals(right);
}