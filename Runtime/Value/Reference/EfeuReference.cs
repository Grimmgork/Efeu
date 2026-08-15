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
    public readonly ulong A;
    public readonly ulong B;
    public readonly ulong C;
    public readonly ulong D;

    public EfeuReference(ulong a, ulong b, ulong c, ulong d)
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

    public readonly void ToBytes(Span<byte> destination)
    {
        if (destination.Length < 32)
            throw new ArgumentException("Destination must be at least 32 bytes.");

        BinaryPrimitives.WriteUInt64LittleEndian(destination[0..8], A);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[8..16], B);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[16..24], C);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[24..32], D);
    }

    public static EfeuReference FromBytes(byte[] bytes)
    {
        if (bytes.Length != 32)
            throw new ArgumentException("Expected 32 bytes.", nameof(bytes));

        return new EfeuReference(
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[0..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..16]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[16..24]),
            BinaryPrimitives.ReadUInt64LittleEndian(bytes[24..32])
        );
    }
}

