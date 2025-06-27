using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Workflows.Data;

namespace Workflows.Json
{
    public class SomeDataTraversalJsonConverter : JsonConverter<SomeDataTraversal>
    {
        public override SomeDataTraversal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString() ?? "";
        }

        public override void Write(Utf8JsonWriter writer, SomeDataTraversal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
