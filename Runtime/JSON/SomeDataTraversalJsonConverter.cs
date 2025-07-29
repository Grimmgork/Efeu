using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Efeu.Runtime.Data;

namespace Efeu.Runtime.Json
{
    public class SomeDataTraversalJsonConverter : JsonConverter<DataTraversal>
    {
        public override DataTraversal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() ?? "";
        }

        public override void Write(Utf8JsonWriter writer, DataTraversal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
