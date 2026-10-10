using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Efeu.Integration.Entities;
using Efeu.Integration.Utils;
using Efeu.Runtime.Value;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueSerializerOptions
{
    public required IEfeuValueWriter Writer { get; set; }

    public required IEfeuValueHasher Hasher { get; set; }
}

public class EfeuValueSerializer
{
    private IEfeuValueWriter writer;
    private IEfeuValueHasher hasher;

    private Dictionary<string, ValueNodeEntity> nodes = new ();
    private List<ValueNodeReferenceEntity> references = [];
    private List<string> rootHashes = [];
    
    public static EfeuValueSerializationResult Serialize(EfeuValue[] roots, EfeuValueSerializerOptions options)
    {
        EfeuValueSerializer serializer = new EfeuValueSerializer(options);
        serializer.Serialize(roots);
        return serializer.End();
    }
    
    private EfeuValueSerializer(EfeuValueSerializerOptions options)
    {
        this.writer = options.Writer;
        this.hasher = options.Hasher;
    }
    
    public static EfeuValueSerializer Begin(EfeuValueSerializerOptions options)
    {
        return new EfeuValueSerializer(options);
    }

    public EfeuValueSerializationResult End()
    {
        return new EfeuValueSerializationResult()
        {
            Hashes = rootHashes.ToArray(),
            Nodes = nodes,
            References = references
        };
    }

    public static Type GetEfeuObjectType(byte code)
    {
        return code switch
        {
            5 => typeof(EfeuString),
            6 => typeof(EfeuDecimal),
            7 => typeof(EfeuArray),
            8 => typeof(EfeuHash),
            9 => typeof(EfeuTime),
            _ => throw new InvalidOperationException()
        };
    }

    private static byte GetEfeuObjectCode(Type type)
    {
        if (type == typeof(EfeuString)) return 5;
        if (type == typeof(EfeuDecimal)) return 6;
        if (type == typeof(EfeuArray)) return 7;
        if (type == typeof(EfeuHash)) return 8;
        if (type == typeof(EfeuTime)) return 9;
        throw new InvalidOperationException();
    }

    public string[] Serialize(EfeuValue[] roots)
    {
        string[] results = new string[roots.Length];
        for (int i = 0; i < roots.Length; i++)
        {
            string hash = Serialize(roots[i]);
            results[i] = hash;
            rootHashes.Add(hash);
        }
        return results;
    }
    
    public string Serialize(EfeuValue value)
    {
        IEnumerable<string> children = [];
        
        writer.Push();
        if (value.Tag == EfeuValueTag.Nil)
        {
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.True)
        {
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.False)
        {
            writer.WriteByte((byte)value.Tag);
        }
        else if (value.Tag == EfeuValueTag.Integer)
        {
            writer.WriteByte((byte)value.Tag);
            writer.WriteInt64(value.AsLong());
        }
        else // object
        {
            EfeuObject obj = value.AsObject();
            writer.WriteByte(GetEfeuObjectCode(obj.GetType()));
            SerializeEfeuObject(obj, out children);
        }
        byte[] payload = writer.Pop();
        if (payload.Length < 32)
        {
            return Convert.ToHexString(payload);
        }
        else
        {
            string hash = Convert.ToHexString(hasher.Hash(payload));
            if (!nodes.ContainsKey(hash))
            {
                nodes.Add(hash, new ValueNodeEntity()
                {
                    Hash = hash,
                    Payload = payload
                });

                foreach (string child in children)
                {
                    if (child.Length >= 64)
                    {
                        this.references.Add(new ValueNodeReferenceEntity()
                        {
                            SourceHash = hash,
                            TargetHash = child
                        });
                    }
                }
            }
            return hash;
        }
    }

    private void SerializeEfeuObject(EfeuObject obj, out IEnumerable<string> children)
    {
        children = [];
        
        if (obj is EfeuString efeuString)
        {
            writer.WriteString(efeuString.ToString());
        }
        else if (obj is EfeuDecimal efeuDecimal)
        {
            writer.WriteShortString(efeuDecimal.ToString());
        }
        else if (obj is EfeuArray efeuArray)
        {
            List<string> childrenHashes = [];
            writer.WriteInt32(efeuArray.Count);
            foreach (EfeuValue value in efeuArray)
            {
                string hash = Serialize(value);
                childrenHashes.Add(hash);
                writer.WriteShortString(hash);
            }
            children = childrenHashes;
        }
        else if (obj is EfeuHash efeuHash)
        {
            List<string> childrenHashes = [];
            writer.WriteInt32(efeuHash.Count());
            foreach (KeyValuePair<string, EfeuValue> entry in efeuHash)
            {
                writer.WriteShortString(entry.Key);
                string hash = Serialize(entry.Value);
                childrenHashes.Add(hash);
                writer.WriteShortString(hash);
            }
            children = childrenHashes;
        }
        else if (obj is EfeuTime efeuTime)
        {
            writer.WriteInt64(efeuTime.Value.ToUnixTimeMilliseconds());
        }
    }
}