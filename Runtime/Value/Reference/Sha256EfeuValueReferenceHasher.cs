using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public sealed class Sha256EfeuValueReferenceHasher : IEfeuValueReferenceHasher, IDisposable
{
    private readonly Stack<IncrementalHash> stack = new Stack<IncrementalHash>();
    private readonly Dictionary<EfeuObject, EfeuReference> cache = new Dictionary<EfeuObject, EfeuReference>();
    private readonly Dictionary<EfeuReference, EfeuValue> lookup = new Dictionary<EfeuReference, EfeuValue>();
    private readonly Action<EfeuValue, EfeuReference>? callback;

    public Sha256EfeuValueReferenceHasher(Action<EfeuValue, EfeuReference>? callback = null)
    {
        this.callback = callback;
    }

    public EfeuReference HashReference(EfeuValue value)
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
            EfeuReference result = EfeuReference.From(objectHashBytes);
            cache.Add(obj, result);
            lookup.Add(result, value);
            callback?.Invoke(value, result);
            return result;
        }
        else
        {
            return EfeuReference.From(value.Tag, value.Integer);
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