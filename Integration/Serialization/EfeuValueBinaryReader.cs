using System;
using System.Collections.Generic;
using System.Linq;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueBinaryReader : IEfeuValueReader
{
    private readonly Stack<ByteReader> payloads = new Stack<ByteReader>();
    private ByteReader top = new ByteReader([]);
    
    private class ByteReader
    {
        private readonly byte[] _data;
        private int _position;

        public ByteReader(byte[] data)
        {
            _data = data;
        }

        public byte ReadByte()
            => _data[_position++];

        public byte[] Read(int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(_data, _position, result, 0, length);
            _position += length;
            return result;
        }
    }
    
    public void Push(byte[] payload)
    {
        top = new ByteReader(payload);
        payloads.Push(top);
    }

    public byte[] ReadBytes(int length)
    {
        return top.Read(length);
    }

    public byte ReadByte()
    {
        return top.ReadByte();
    }

    public void Pop()
    {
        payloads.Pop();
        if (payloads.Any())
        {
            top = payloads.Peek();
        }
        else
        {
            top = new ByteReader([]);
        }
    }
}