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

public readonly struct EfeuValueReference : IEquatable<EfeuValueReference>
{
    public readonly EfeuValueTag Tag;
    public readonly long Integer;
    public readonly ulong A;
    public readonly ulong B;
    public readonly ulong C;
    public readonly ulong D;
    
    private EfeuValueReference(EfeuValueTag tag, long integer, ulong a, ulong b, ulong c, ulong d)
    {
        Tag = tag;
        Integer = integer;
        A = a;
        B = b;
        C = c;
        D = d;
    }

    public bool Equals(EfeuValueReference other)
        => A == other.A
           && B == other.B
           && C == other.C
           && D == other.D
           && Tag == other.Tag
           && Integer == other.Integer;

    public override bool Equals(object? obj)
        => obj is EfeuValueReference other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Tag, Integer, A, B, C, D);

    public static EfeuValueReference From(EfeuValueTag tag, long integer)
    {
        return new EfeuValueReference(tag, integer, 0, 0, 0, 0);
    }

    public static EfeuValueReference From(byte[] objectHashBytes)
    {
        if (objectHashBytes.Length != 32)
            throw new ArgumentException("Object hash length must be 32 bytes.");
        
        return new EfeuValueReference(
            EfeuValueTag.Object,
            0,
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[0..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[8..16]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[16..24]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[24..32])
        );
    }

    public void Serialize(Span<byte> destination)
    {
        if (destination.Length < 41)
            throw new ArgumentException("Destination must be at least 41 bytes.");

        destination[0] = (byte)Tag;
        BinaryPrimitives.WriteInt64LittleEndian(destination[1..9], Integer);
        Span<byte> objectHashBytes = destination[9..41];
        BinaryPrimitives.WriteUInt64LittleEndian(objectHashBytes[0..8], A);
        BinaryPrimitives.WriteUInt64LittleEndian(objectHashBytes[8..16], B);
        BinaryPrimitives.WriteUInt64LittleEndian(objectHashBytes[16..24], C);
        BinaryPrimitives.WriteUInt64LittleEndian(objectHashBytes[24..32], D);
    }
    
    public static EfeuValueReference Deserialize(byte[] bytes)
    {
        if (bytes.Length != 41)
            throw new ArgumentException("Expected 41 bytes.", nameof(bytes));
        
        Span<byte> objectHashBytes = bytes[9..41];
        return new EfeuValueReference(
            (EfeuValueTag)bytes[0],
            BinaryPrimitives.ReadInt64LittleEndian(bytes[1..9]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[0..8]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[8..16]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[16..24]),
            BinaryPrimitives.ReadUInt64LittleEndian(objectHashBytes[24..32])
        );
    }
}

