using System;
using System.Buffers.Binary;
using System.Text;

namespace Efeu.Integration.Utils.Serialization;

public interface IEfeuValueWriter
{
    public void Push();

    public byte[] Pop();
    
    public void WriteBytes(byte[] bytes);

    public void WriteByte(byte value);
    
    void WriteInt32(int value)
    {
        byte[] bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        WriteBytes(bytes);
    }

    void WriteUInt32(uint value)
    {
        byte[] bytes = new byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        WriteBytes(bytes);
    }

    void WriteInt64(long value)
    {
        byte[] bytes = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        WriteBytes(bytes);
    }

    void WriteUInt64(ulong value)
    {
        byte[] bytes = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        WriteBytes(bytes);
    }

    void WriteString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        WriteInt32(value.Length);
        WriteBytes(Encoding.UTF8.GetBytes(value));
    }

    void WriteDouble(double value)
    {
        byte[] bytes = new byte[sizeof(double)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, BitConverter.DoubleToInt64Bits(value));
        WriteBytes(bytes);
    }
}
