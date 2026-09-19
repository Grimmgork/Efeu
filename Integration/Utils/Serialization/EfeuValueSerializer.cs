using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Efeu.Integration.Entities;
using Efeu.Integration.Utils;
using Efeu.Runtime.Value;
using Efeu.Runtime.Value.Reference;

namespace Efeu.Integration.Utils.Serialization;

public class EfeuValueSerializerOptions
{
    public Action<string, byte[]> Visit = (_, _) => { };

    public required IEfeuValueWriter Writer { get; set; }

    public required IEfeuValueHasher Hasher { get; set; }
}


public class EfeuValueSerializer
{
    private EfeuValue root;
    private IEfeuValueWriter writer;
    private IEfeuValueHasher hasher;
    private Action<string, byte[]> visit;
    private HashSet<string> visitedHashes = new();
    
    public static string Serialize(EfeuValue root, EfeuValueSerializerOptions options)
    {
        EfeuValueSerializer serializer = new EfeuValueSerializer(root, options);
        return serializer.Serialize(root);
    }

    private EfeuValueSerializer(EfeuValue root, EfeuValueSerializerOptions options)
    {
        this.root = root;
        this.writer = options.Writer;
        this.hasher = options.Hasher;
        this.visit = options.Visit;
    }

    private string Serialize(EfeuValue value)
    {
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
        else
        {
            writer.WriteByte((byte)value.Tag);
            EfeuObject obj = value.AsObject();
            Serialize(obj);
        }
        byte[] payload = writer.Pop();
        string hash = Convert.ToHexString(hasher.Hash(payload));
        if (!visitedHashes.Contains(hash))
        {
            visit(hash, payload);
            visitedHashes.Add(hash);
        }
        return hash;
    }

    private void Serialize(EfeuObject obj)
    {
        if (obj is EfeuString efeuString)
        {
            writer.WriteString(nameof(EfeuString));
            writer.WriteString(efeuString.ToString());
        }
        
        if (obj is EfeuDecimal efeuDecimal)
        {
            writer.WriteString(nameof(EfeuDecimal));
            writer.WriteString(efeuDecimal.ToString());
        }

        if (obj is EfeuArray efeuArray)
        {
            writer.WriteString(nameof(EfeuArray));
            writer.WriteInt32(efeuArray.Count);
            foreach (EfeuValue value in efeuArray)
            {
                string hash = Serialize(value);
                writer.WriteString(hash);
            }
        }
    }
}