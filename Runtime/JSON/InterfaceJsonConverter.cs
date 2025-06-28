using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.Json
{
    public class InterfaceJsonConverter<TInterface> : JsonConverter<TInterface>
    {
        private Type[] implementations;

        public InterfaceJsonConverter(params Type[] implementations)
        {
            this.implementations = implementations;
        }

        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            JsonDocument json = JsonDocument.ParseValue(ref reader);
            JsonElement root = json.RootElement;

            string typeName = root.GetProperty("$type").ToString();
            Type targetType = implementations.First(i => i.Name == typeName);

            // TODO
            return (TInterface)JsonSerializer.Deserialize(root, targetType, options)!;
        }

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Type targetType = value.GetType();
            string typeName = targetType.Name;

            writer.WriteStartObject();
            writer.WriteString("$type", typeName);

            foreach (PropertyInfo prop in targetType.GetProperties())
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.GetValue(value), options);
            }

            writer.WriteEndObject();
        }
    }
}
