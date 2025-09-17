using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Efeu.Runtime.Serialization
{
    public class ReferencePreservingEfeuValueFormatter : IMessagePackFormatter<EfeuValue>
    {
        private readonly Dictionary<EfeuObject, int> seenObjects = new Dictionary<EfeuObject, int>();

        private readonly Dictionary<int, EfeuObject> seenIds = new Dictionary<int, EfeuObject>();

        private readonly IWellKnownTypeRegistry typeRegistry;

        public ReferencePreservingEfeuValueFormatter(IWellKnownTypeRegistry typeRegistry)
        {
            this.typeRegistry = typeRegistry;
        }

        public ReferencePreservingEfeuValueFormatter()
        {
            this.typeRegistry = new WellKnownTypeRegistry();
        }

        public EfeuValue Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            EfeuValueTag tag = (EfeuValueTag)reader.ReadByte();
            if (tag == EfeuValueTag.Nil)
            {
                return EfeuValue.Nil();
            }
            else if (tag == EfeuValueTag.True)
            {
                return true;
            }
            else if (tag == EfeuValueTag.False)
            {
                return false;
            }
            else if (tag == EfeuValueTag.Integer)
            {
                return reader.ReadInt64();
            }
            else if (tag == EfeuValueTag.Object)
            {
                if (reader.NextMessagePackType == MessagePackType.Integer)
                {
                    // read reference
                    int id = reader.ReadInt32();
                    return seenIds[id];
                }
                else
                {
                    // read object
                    string typeName = reader.ReadString()!;
                    int id = reader.ReadInt32();
                    Type typeToDeserialize = typeRegistry.GetTypeByName(typeName);
                    EfeuObject efeuObject = (EfeuObject)MessagePackSerializer.Deserialize(typeToDeserialize, ref reader, options)!;
                    seenIds[id] = efeuObject;
                    return efeuObject;
                }
            }

            throw new InvalidOperationException();
        }

        public void Serialize(ref MessagePackWriter writer, EfeuValue value, MessagePackSerializerOptions options)
        {
            writer.WriteUInt8((byte)value.Tag);
            if (value.Tag == EfeuValueTag.Integer)
            {
                writer.WriteInt64(value.ToLong());
            }
            else if (value.Tag == EfeuValueTag.Object)
            {
                if (seenObjects.ContainsKey(value.AsObject()))
                {
                    // write reference
                    writer.WriteInt32(seenObjects[value.AsObject()]);
                }
                else
                {
                    // write object
                    writer.WriteString(Encoding.UTF8.GetBytes(typeRegistry.GetNameOfType(value.AsObject().GetType())));
                    int id = seenObjects.Count + 1;
                    writer.WriteInt32(id);
                    seenObjects[value.AsObject()] = id;
                    MessagePackSerializer.Serialize(value.AsObject().GetType(), ref writer, value.AsObject(), options);
                }
            }
        }
    }
}
