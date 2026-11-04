using System;

namespace Efeu.Integration.Utils.Serialization;

public readonly struct EfeuValueHash : IEquatable<EfeuValueHash>
{
    private readonly string? _value;

    public EfeuValueHash(string? value)
    {
        _value = value;
    }
    
    public static EfeuValueHash Empty => default;
    
    public int Length => _value?.Length ?? 2;
    
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    public bool Equals(EfeuValueHash other)
        => StringComparer.Ordinal.Equals(_value ?? string.Empty, other._value ?? string.Empty);

    public override bool Equals(object? obj)
        => obj is EfeuValueHash other && Equals(other);

    public override int GetHashCode()
        => _value is null
            ? StringComparer.Ordinal.GetHashCode(string.Empty)
            : StringComparer.Ordinal.GetHashCode(_value);

    public override string ToString()
        => _value ?? string.Empty;

    public static explicit operator EfeuValueHash(string value)
        => new(value);

    public static implicit operator string(EfeuValueHash value)
        => value._value ?? string.Empty;

    public static bool operator ==(EfeuValueHash left, EfeuValueHash right)
        => left.Equals(right);

    public static bool operator !=(EfeuValueHash left, EfeuValueHash right)
        => !left.Equals(right);
}