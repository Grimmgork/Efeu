using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueBinaryWriter : IEfeuValueWriter
{
    private readonly Stack<ArrayBufferWriter<byte>> buffers = new Stack<ArrayBufferWriter<byte>>();
    private ArrayBufferWriter<byte> top = new ArrayBufferWriter<byte>();
        
    public void Push()
    {
        top = new ArrayBufferWriter<byte>();
        buffers.Push(top);
    }

    public byte[] Pop()
    {
        ArrayBufferWriter<byte> buffer = buffers.Pop();
        if (buffers.Any())
        {
            top = buffers.Peek();
        }
        else
        {
            top = new ArrayBufferWriter<byte>();
        }
        return buffer.WrittenMemory.ToArray(); // todo optimize for span
    }

    public void WriteBytes(byte[] bytes)
    {
        top.Write(bytes);
    }

    public void WriteByte(byte value)
    {
        Span<byte> span = stackalloc byte[1];
        span[0] = value;
        top.Write(span);
    }

    public byte[] ReadBytes(int length)
    {
        throw new System.NotImplementedException();
    }

    public byte ReadByte()
    {
        throw new System.NotImplementedException();
    }
}