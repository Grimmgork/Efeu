using System;

namespace Efeu.Integration.Utils.Serialization;

public interface IEfeuValueHasher
{
    public Span<byte> Hash(Span<byte> bytes);
}