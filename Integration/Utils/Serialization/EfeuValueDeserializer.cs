using System;
using System.Collections.Generic;
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
    private Dictionary<string, ValueNodeEntity> nodes;
    
    private EfeuValueDeserializer(EfeuValueSerializationResult result, EfeuValueDeserializerOptions options)
    {
        this.reader = options.Reader;
        this.nodes = result.Nodes;
    }
    
    public static EfeuValue Deserialize(EfeuValueSerializationResult result, EfeuValueDeserializerOptions options)
    {
        EfeuValueDeserializer deserializer = new EfeuValueDeserializer(result, options);
        return deserializer.Deserialize(result.Hash);
    }

    private EfeuValue Deserialize(string hash)
    {
        if (cache.TryGetValue(hash, out var value))
        {
            return value;
        }

        byte[] payload = [];
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
            result = DeserializeObject(type, payload);
        }
        
        reader.Pop();
        cache.Add(hash, result);
        return result;
    }

    private EfeuObject DeserializeObject(Type type, byte[] payload)
    {
        if (type == typeof(EfeuString))
        {
            string str = reader.ReadString();
            return new EfeuString(str);
        }
        
        if (type == typeof(EfeuDecimal))
        {
            string str = reader.ReadString();
            decimal dec = decimal.Parse(str);
            return new EfeuDecimal(dec);
        }
        
        if (type == typeof(EfeuArray))
        {
            int length = reader.ReadInt32();
            EfeuValue[] items = new EfeuValue[length];
            for (int i = 0; i < length; i++)
            {
                string hash = reader.ReadString();
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
                string key = reader.ReadString();
                string hash = reader.ReadString();
                EfeuValue value = Deserialize(hash);
                entries[i] = new KeyValuePair<string, EfeuValue>(key, value);
            }
            return new EfeuHash(entries);
        }

        throw new InvalidOperationException();
    }
}