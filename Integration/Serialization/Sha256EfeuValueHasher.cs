using System;
using System.Security.Cryptography;

namespace Efeu.Integration.Utils.Serialization;

public sealed class Sha256EfeuValueHasher : IEfeuValueHasher
{
    public Span<byte> Hash(Span<byte> bytes)
    {
        byte[] hash = new byte[SHA256.HashSizeInBytes];
        SHA256.HashData(bytes, hash);
        return hash;
    }
}