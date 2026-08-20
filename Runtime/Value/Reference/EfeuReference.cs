using SharpCompress.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public readonly struct EfeuReference : IEquatable<EfeuReference>
{
    public readonly EfeuValueTag Tag;
    public readonly byte[] Hash = [];
    public readonly byte[] Payload = [];
    
    private EfeuReference(ulong a, ulong b, ulong c, ulong d)
    {
        A = a;
        B = b;
        C = c;
        D = d;
    }

    public bool Equals(EfeuReference other)
        => A == other.A
           && B == other.B
           && C == other.C
           && D == other.D;

    public override bool Equals(object? obj)
        => obj is EfeuReference other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(A, B, C, D);

    public static EfeuReference FromBytes(byte[] bytes)
    {
        if (bytes.Length != SizeInBytes)
            throw new ArgumentException("Object hash length must be 32 bytes.");
        
        return new EfeuReference(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[0..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..16]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[16..24]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[24..32])
        );
    }

    public void ToBytes(Span<byte> destination)
    {
        if (destination.Length < SizeInBytes)
            throw new ArgumentException("Destination must be at least 41 bytes.");

        BinaryPrimitives.WriteUInt64LittleEndian(destination[0..8], A);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..16], B);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..24], C);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..32], D);
    }
    
    public override string ToString()
    {
        Span<byte> bytes = stackalloc byte[SizeInBytes];
        ToBytes(bytes);
        return Convert.ToHexString(bytes);
    }

    public static EfeuReference FromString(string str)
    {
        return FromBytes(Convert.FromHexString(str));
    }
}

