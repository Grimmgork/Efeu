using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public sealed class Sha256EfeuValueReferenceHasher : IEfeuValueReferenceHasher, IDisposable
{
    private readonly Stack<IncrementalHash> stack = new Stack<IncrementalHash>();
    private readonly Dictionary<EfeuObject, EfeuValueReference> cache = new Dictionary<EfeuObject, EfeuValueReference>();
    private readonly Dictionary<EfeuValueReference, EfeuValue> lookup = new Dictionary<EfeuValueReference, EfeuValue>();
    private readonly Action<EfeuValue, EfeuValueReference>? callback;

    public Sha256EfeuValueReferenceHasher(Action<EfeuValue, EfeuValueReference>? callback = null)
    {
        this.callback = callback;
    }

    public EfeuValueReference HashReference(EfeuValue value)
    {
        if (value.Tag  == EfeuValueTag.Object)
        {
            EfeuObject obj = value.AsObject();
            if (cache.TryGetValue(obj, out var entry))
            {
                return entry;
            }
            
            BeginHash();
            obj.WriteReference(this);
            byte[] objectHashBytes = EndHash();
            EfeuValueReference result = EfeuValueReference.From(objectHashBytes);
            cache.Add(obj, result);
            lookup.Add(result, value);
            callback?.Invoke(value, result);
            return result;
        }
        else
        {
            return EfeuValueReference.From(value.Tag, value.Integer);
        }
    }

    public void WriteByte(byte value)
    {
        Span<byte> b = stackalloc byte[1] { value };
        stack.Peek().AppendData(b);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        stack.Peek().AppendData(bytes);
    }

    private void BeginHash()
    {
        stack.Push(IncrementalHash.CreateHash(HashAlgorithmName.SHA256));
    }

    private byte[] EndHash()
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