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
        public override EfeuRuntimeScope Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            Guid id = Guid.Empty;
            ImmutableDictionary<string, EfeuValue> constants =
                ImmutableDictionary<string, EfeuValue>.Empty;

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? property = reader.GetString();
                if (property == nameof(EfeuRuntimeScope.Id))
                {
                    reader.Read();
                    string? value = reader.GetString();
                    if (value == null)
                        throw new JsonException();

                    id = Guid.Parse(value);
                }
                else if (property == nameof(EfeuRuntimeScope.Constants))
                {
                    reader.Read();
                    constants = JsonSerializer.Deserialize<ImmutableDictionary<string, EfeuValue>>(ref reader, options) ?? 
                        ImmutableDictionary<string, EfeuValue>.Empty;
                }
                else
                {
                    reader.Skip();
                }

                reader.Read();
            }

            return new EfeuRuntimeScope(id, constants);
        }

        public override void Write(Utf8JsonWriter writer, EfeuRuntimeScope value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(EfeuRuntimeScope.Id));
            JsonSerializer.Serialize(writer, value.Id, options);
            writer.WritePropertyName(nameof(EfeuRuntimeScope.Constants));
            JsonSerializer.Serialize(writer, value.Constants, options);
            writer.WriteEndObject();
        }
    }
}
