using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Efeu.Integration.Entities;
using Efeu.Integration.Utils;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Utils;

public class EfeuValueDeserializer
{
    private byte[] root;
    private IEfeuValueWriter writer;
    
    private EfeuValue result;

    private EfeuValueDeserializer(byte[] root, EfeuValueDeserializerOptions options)
    {
        this.writer = options.Writer;
        this.root = root;
    }
    
    public static EfeuValue Deserialize(byte[] root, EfeuValueDeserializerOptions options)
    {
        EfeuValueDeserializer deserializer = new EfeuValueDeserializer(root, options);
        deserializer.Deserialize(root);
        return deserializer.result;
    }

    private void Deserialize(byte[] payload)
    {
        
    }
}

public class EfeuValueSerializer
{
    private EfeuValue root;
    private IEfeuValueWriter writer;
    private IEfeuValueHasher hasher;
    private Action<byte[], byte[]> visit;

    private string rootHash;
    
    public static string Serialize(EfeuValue root, EfeuValueSerializerOptions options)
    {
        EfeuValueSerializer serializer = new EfeuValueSerializer(root, options);
        serializer.Serialize(root);
        return serializer.rootHash;
    }
    
    private EfeuValueSerializer(EfeuValue root, EfeuValueSerializerOptions options)
    {
        this.root = root;
        this.writer = options.Writer;
        this.hasher = options.Hasher;
        this.visit = options.Visit;
    }

    private void Serialize(EfeuValue value)
    {
        hasher.Begin();
        if (value.Tag == EfeuValueTag.Nil)
        {
            hasher.WriteByte((byte)value.Tag);
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.True)
        {
            hasher.WriteByte((byte)value.Tag);
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.False)
        {
            hasher.WriteByte((byte)value.Tag);
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.Integer)
        {
            hasher.WriteByte((byte)value.Tag);
            hasher.WriteInt64(value.AsLong());
        }
        Span<byte> hash = hasher.End();
    }

    private void Serialize(EfeuHash value)
    {
        
    }

    private void Serialize(EfeuArray value)
    {
        
    }

    private void Serialize(EfeuString value)
    {
        
    }
}

public interface IEfeuValueWriter
{
    public void WriteBytes(byte[] bytes);
    
    public void WriteByte(byte value);
    
    public byte[] ReadBytes(int length);
}

public interface IEfeuValueHasher
{
    public void Begin();
    
    public void WriteByte(byte value);
    
    public void WriteBytes(Span<byte> buffer);
    
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

    public Span<byte> End();
}

public sealed class Sha256EfeuValueHasher : IEfeuValueHasher, IDisposable
{
    private readonly Stack<IncrementalHash> stack = new Stack<IncrementalHash>();
    
    public void WriteByte(byte value)
    {
        Span<byte> b = stackalloc byte[1] { value };
        stack.Peek().AppendData(b);
    }

    public void WriteBytes(Span<byte> bytes)
    {
        stack.Peek().AppendData(bytes);
    }

    public void Begin()
    {
        stack.Push(IncrementalHash.CreateHash(HashAlgorithmName.SHA256));
    }

    public Span<byte> End()
    {
        IncrementalHash hash = stack.Pop();
        byte[] result = hash.GetHashAndReset();
        hash.Dispose();
        return result;
    }

    public void Dispose()
    {
        foreach (var hash in stack)
            hash.Dispose();
        
        stack.Clear();
    }
}

public class EfeuValueSerializerOptions
{
    public Action<byte[], byte[]> Visit = (_,_) => { };
    
    public required IEfeuValueWriter Writer { get; set; }
    
    public required IEfeuValueHasher Hasher { get; set; }
}

public class EfeuValueBinaryWriter : IEfeuValueWriter
{
    public void WriteBytes(byte[] bytes)
    {
        throw new NotImplementedException();
    }

    public void WriteByte(byte value)
    {
        throw new NotImplementedException();
    }

    public byte[] ReadBytes(int length)
    {
        throw new NotImplementedException();
    }
}

public class EfeuValueDeserializerOptions
{
    public Func<byte[], byte[]> Resolve = (_) => [];
    
    public required IEfeuValueWriter Writer { get; set; }
}