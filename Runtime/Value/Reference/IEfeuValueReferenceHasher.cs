using System;
using System.Buffers.Binary;
using System.Text;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public interface IEfeuValueReferenceHasher
{
    public EfeuReference HashReference(EfeuValue value);

    public void WriteByte(byte value);

    public void WriteBytes(ReadOnlySpan<byte> bytes);

    public void WriteValue(EfeuValue value)
    {
        EfeuReference reference = HashReference(value);
        WriteByte((byte)reference.Tag);
        WriteInt64(reference.Integer);
        WriteUInt64(reference.A);
        WriteUInt64(reference.B);
        WriteUInt64(reference.C);
        WriteUInt64(reference.D);
    }

    public void WriteUInt64(ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteInt64(long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteDouble(double value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        this.WriteBytes(buffer);
    }

    public void WriteString(string value)
    {
        if (value == null)
            throw new InvalidOperationException();

        int byteCount = Encoding.UTF8.GetByteCount(value);
        this.WriteInt64(byteCount);

        Span<byte> buffer = byteCount <= 256
            ? stackalloc byte[byteCount]
            : new byte[byteCount];

        Encoding.UTF8.GetBytes(value, buffer);
        this.WriteBytes(buffer);
    }
}
