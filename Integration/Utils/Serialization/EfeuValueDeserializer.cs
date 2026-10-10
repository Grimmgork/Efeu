using System;
using System.Collections.Generic;
using System.Linq;
using Efeu.Integration.Entities;
using Efeu.Runtime.Value;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueDeserializerOptions
{
    public required IEfeuValueReader Reader;
}

public class EfeuValueDeserializer
{
    private IEfeuValueReader reader;
    private Dictionary<string, EfeuValue> cache = new Dictionary<string, EfeuValue>();
    private Dictionary<string, ValueNodeEntity> nodes = new Dictionary<string, ValueNodeEntity>();
    
    private EfeuValueDeserializer(EfeuValueDeserializerOptions options)
    {
        this.reader = options.Reader;
    }

    public string[] GetNotDeserialized(IEnumerable<string> hashes)
    {
        return hashes.Where(i => i.Length < 64 || cache.ContainsKey(i)).ToArray();
    }
    
    public static EfeuValue[] Deserialize(EfeuValueSerializationResult result, EfeuValueDeserializerOptions options)
    {
        EfeuValueDeserializer deserializer = new EfeuValueDeserializer(options);
        return deserializer.Deserialize(result);
    }

    public static EfeuValueDeserializer Begin(EfeuValueDeserializerOptions options)
    {
        return new EfeuValueDeserializer(options);
    }

    public EfeuValue[] Deserialize(EfeuValueSerializationResult result)
    {
        EfeuValue[] results = new EfeuValue[result.Hashes.Length];
        nodes = result.Nodes;
        for (int i = 0; i < result.Hashes.Length; i++)
        {
            results[i] = Deserialize(result.Hashes[i]);
        }
        return results;
    }

    public EfeuValue[] Resolve(string[] hashes)
    {
        EfeuValue[] results = new EfeuValue[hashes.Length];
        for (int i = 0; i < hashes.Length; i++)
        {
            results[i] = Deserialize(hashes[i]);
        }
        return results;
    }
    
    private EfeuValue Deserialize(string hash)
    {
        if (cache.TryGetValue(hash, out var value))
        {
            return value;
        }

        byte[] payload;
        if (hash.Length < 64)
        {
            payload = Convert.FromHexString(hash);
        }
        else
        {
            payload = nodes[hash].Payload;
        }
        
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
        else
        {
            Type type = EfeuValueSerializer.GetEfeuObjectType((byte)tag);
            result = DeserializeObject(type);
        }
        
        reader.Pop();
        cache.Add(hash, result);
        return result;
    }

    private EfeuObject DeserializeObject(Type type)
    {
        if (type == typeof(EfeuString))
        {
            string str = reader.ReadString();
            return new EfeuString(str);
        }
        
        if (type == typeof(EfeuDecimal))
        {
            string str = reader.ReadShortString();
            decimal dec = decimal.Parse(str);
            return new EfeuDecimal(dec);
        }
        
        if (type == typeof(EfeuArray))
        {
            int length = reader.ReadInt32();
            EfeuValue[] items = new EfeuValue[length];
            for (int i = 0; i < length; i++)
            {
                string hash = reader.ReadShortString();
                items[i] = Deserialize(hash);
            }
            
            return new EfeuArray(items);
        }

        if (type == typeof(EfeuHash))
        {
            int length = reader.ReadInt32();
            KeyValuePair<string, EfeuValue>[] entries = new KeyValuePair<string, EfeuValue>[length];
            for (int i = 0; i < length; i++)
            {
                string key = reader.ReadShortString();
                string hash = reader.ReadShortString();
                EfeuValue value = Deserialize(hash);
                entries[i] = new KeyValuePair<string, EfeuValue>(key, value);
            }
            return new EfeuHash(entries);
        }

        if (type == typeof(EfeuTime))
        {
            Int64 time = reader.ReadInt64();
            return new EfeuTime(DateTimeOffset.FromUnixTimeMilliseconds(time));
        }

        throw new InvalidOperationException();
    }
}