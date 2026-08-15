using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Runtime.Value.Reference;

public sealed class Sha256EfeuReferenceHasher : IEfeuReferenceHasher, IDisposable
{
    private readonly Stack<IncrementalHash> stack = new Stack<IncrementalHash>();
    private readonly Dictionary<EfeuObject, EfeuReference> cache = new Dictionary<EfeuObject, EfeuReference>();
    private readonly Dictionary<EfeuReference, EfeuValue> lookup = new Dictionary<EfeuReference, EfeuValue>();
    private readonly Action<EfeuValue, EfeuReference>? callback;

    public Sha256EfeuReferenceHasher(Action<EfeuValue, EfeuReference>? callback = null)
    {
        this.callback = callback;
    }

    public EfeuReference HashReference(EfeuValue value)
    {
        if (value.Tag == EfeuValueTag.Object)
        {
            EfeuObject obj = value.AsObject();
            if (cache.TryGetValue(obj, out var entry))
            {
                return entry;
            }
            else
            {
                Push();
                value.AsObject().WriteReference(this);
                EfeuReference result = Pop();
                cache.Add(obj, result);
                lookup.Add(result, value);
                callback?.Invoke(value, result);
                return result;
            }
        }

        Push();
        WriteByte(Convert.ToByte(value.Tag));
        if (value.Tag == EfeuValueTag.Integer)
        {
            (this as IEfeuReferenceHasher)
                .WriteInt64(value.AsLong());
        }

        EfeuReference res = Pop();
        lookup.Add(res, value);
        callback?.Invoke(value, res);
        return res;
    }

    public void WriteByte(byte value)
    {
        Span<byte> b = stackalloc byte[1];
        b[0] = value;
        stack.Peek().AppendData(b);
    }

    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        stack.Peek().AppendData(bytes);
    }

    private void Push()
    {
        stack.Push(IncrementalHash.CreateHash(HashAlgorithmName.SHA256));
    }

    private EfeuReference Pop()
    {
        IncrementalHash hash = stack.Pop();
        EfeuReference result = EfeuReference.FromBytes(hash.GetHashAndReset());
        hash.Dispose();
        return result;
    }

    public void Dispose()
    {
        foreach (var hash in stack)
            hash.Dispose();
    }
}