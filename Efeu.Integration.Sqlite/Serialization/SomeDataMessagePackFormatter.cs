using Efeu.Runtime.Data;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static LinqToDB.Common.Configuration;

namespace Efeu.Integration.Sqlite.Serialization
{
    public class SomeDataMessagePackFormatter : IMessagePackFormatter<SomeData>
    {
        public SomeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            if (count != 4) throw new MessagePackSerializationException("Invalid Data format");

            var type = (WorkflowDataType)reader.ReadInt32();
            object? scalarValue = MessagePackSerializer.Deserialize<object?>(ref reader, options);
            IReadOnlyCollection<SomeData> arrayItems = MessagePackSerializer.Deserialize<IReadOnlyCollection<SomeData>>(ref reader, options);
            IReadOnlyDictionary<string, SomeData> structFields = MessagePackSerializer.Deserialize<IReadOnlyDictionary<string, SomeData>>(ref reader, options);
            return new SomeData(type, scalarValue, arrayItems, structFields);
        }

        public void Serialize(ref MessagePackWriter writer, SomeData value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write((int)value.DataType);
            MessagePackSerializer.Serialize(ref writer, value.Value, options);
            MessagePackSerializer.Serialize(ref writer, value.Fields, options);
            MessagePackSerializer.Serialize(ref writer, value.Items, options);
        }
    }
}
