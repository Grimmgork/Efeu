using SharpCompress.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Value
{
    public struct EfeuReference
    {
        public byte[] Bytes = [];

        public EfeuReference(byte[] hash)
        {
            Bytes = hash;
        }
    }

    public interface IEfeuReferenceHasher
    {
        public EfeuReference HashReference(EfeuValue value);

        public void WriteByte(byte value);
        
        public void WriteBytes(ReadOnlySpan<byte> bytes);

        public void WriteReference(EfeuValue value)
        {
            EfeuReference reference = HashReference(value);
            WriteBytes(reference.Bytes);
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

    public sealed class Sha256EfeuReferenceHasher : IEfeuReferenceHasher, IDisposable
    {
        private readonly Stack<IncrementalHash> stack = new Stack<IncrementalHash>();
        private Dictionary<EfeuObject, EfeuReference> Cache = new Dictionary<EfeuObject, EfeuReference>();
        private Dictionary<EfeuReference, EfeuValue> Lookup = new Dictionary<EfeuReference, EfeuValue>();

        public EfeuReference HashReference(EfeuValue value)
        {
            if (value.Tag == EfeuValueTag.Object)
            {
                EfeuObject obj = value.AsObject();
                if (Cache.TryGetValue(obj, out var entry))
                {
                    return entry;
                }
                else
                {
                    Push();
                    value.AsObject().WriteReference(this);
                    EfeuReference result = Pop();
                    Cache.Add(obj, result);
                    Lookup.Add(result, value);
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
            Lookup.Add(res, value);
            return Pop();
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
            EfeuReference result = new EfeuReference(hash.GetHashAndReset());
            hash.Dispose();
            return result;
        }

        public void Dispose()
        {
            foreach (var hash in stack)
                hash.Dispose();
        }
    }
}
