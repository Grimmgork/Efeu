using Efeu.Runtime.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.Json.Converters
{
    internal class EfeuValueEncodedJsonConverter : JsonConverter<EfeuValue>
    {
        public override EfeuValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, EfeuValue value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
