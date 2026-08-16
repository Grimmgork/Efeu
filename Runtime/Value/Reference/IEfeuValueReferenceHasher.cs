using System;
using System.Buffers.Binary;
using System.Text;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public interface IEfeuValueReferenceHasher
{
    public EfeuValueReference HashReference(EfeuValue value);

    public void WriteByte(byte value);

    public void WriteBytes(ReadOnlySpan<byte> bytes);

    public void WriteReference(EfeuValue value)
    {
        EfeuValueReference valueReference = HashReference(value);
        WriteByte((byte)valueReference.Tag);
        WriteInt64(valueReference.Integer);
        WriteUInt64(valueReference.A);
        WriteUInt64(valueReference.B);
        WriteUInt64(valueReference.C);
        WriteUInt64(valueReference.D);
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
