using Efeu.Runtime.Value;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.Json.Converters
{
    public class EfeuRuntimeScopeJsonConverter : JsonConverter<EfeuRuntimeScope>
    {
        public override EfeuRuntimeScope? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ImmutableDictionary<string, EfeuValue>? constants = JsonSerializer.Deserialize<ImmutableDictionary<string, EfeuValue>>(ref reader, options);
            if (constants == null)
            {
                return null;
            }
            else
            {
                return new EfeuRuntimeScope(constants);
            }
        }

        public override void Write(Utf8JsonWriter writer, EfeuRuntimeScope value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Constants, options);
        }
    }
}
