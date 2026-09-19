using System;
using System.Buffers.Binary;
using System.Text;

namespace Efeu.Integration.Utils.Serialization;

public interface IEfeuValueReader
{
    public void Push(byte[] payload);
    
    public byte[] ReadBytes(int length);
    
    public byte ReadByte();
    
    public int ReadInt32()
        => BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(4));

    public uint ReadUInt32()
        => BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(4));

    public long ReadInt64()
        => BinaryPrimitives.ReadInt64LittleEndian(ReadBytes(8));

    public ulong ReadUInt64()
        => BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(8));
    
    public double ReadDouble()
        => BitConverter.Int64BitsToDouble(ReadInt64());

    public string ReadString()
        => Encoding.UTF8.GetString(ReadBytes(ReadInt32()));
    
    public void Pop();
}