using System;
using System.Collections.Generic;
using Efeu.Runtime.Value;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueDeserializerOptions
{
    public Func<string, byte[]> Resolve = (_) => [];

    public required IEfeuValueReader Reader { get; set; }
}

public class EfeuValueDeserializer
{
    private IEfeuValueReader reader;
    private Dictionary<string, EfeuValue> cache = new Dictionary<string, EfeuValue>();
    private Func<string, byte[]> resolve;
    
    private EfeuValueDeserializer(string rootHash, EfeuValueDeserializerOptions options)
    {
        this.reader = options.Reader;
        this.resolve =  options.Resolve;
    }

    public static EfeuValue Deserialize(string hash, EfeuValueDeserializerOptions options)
    {
        EfeuValueDeserializer deserializer = new EfeuValueDeserializer(hash, options);

        return deserializer.Deserialize(hash);
    }

    private EfeuValue Deserialize(string hash)
    {
        if (cache.TryGetValue(hash, out var value))
        {
            return value;
        }

        byte[] payload = resolve(hash);
        EfeuValue result = EfeuValue.Nil();
        
        reader.Push(payload);
        EfeuValueTag tag = (EfeuValueTag)reader.ReadByte();

        if (tag == EfeuValueTag.Nil)
        {
            
        }
        else if (tag == EfeuValueTag.False)
        {
            result = false;
        }
        else if (tag == EfeuValueTag.True)
        {
            result = true;
        }
        else if (tag == EfeuValueTag.Integer)
        {
            result = reader.ReadInt64();
        }
        else if (tag == EfeuValueTag.Object)
        {
            result = DeserializeObject(payload);
        }
        
        reader.Pop();
        cache.Add(hash, result);
        return result;
    }

    private EfeuObject DeserializeObject(byte[] payload)
    {
        string type = reader.ReadString();
        if (type == nameof(EfeuString))
        {
            string str = reader.ReadString();
            return new EfeuString(str);
        }
        
        if (type == nameof(EfeuDecimal))
        {
            string str = reader.ReadString();
            decimal dec = decimal.Parse(str);
            return new EfeuDecimal(dec);
        }
        
        if (type == nameof(EfeuArray))
        {
            int length = reader.ReadInt32();
            EfeuValue[] items = new EfeuValue[length];
            for (int i = 0; i < length; i++)
            {
                string str = reader.ReadString();
                items[i] = Deserialize(str);
            }
            
            return new EfeuArray(items);
        }

        throw new InvalidOperationException();
    }
}