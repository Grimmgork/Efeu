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
    public class SomeDataJsonConverter : JsonConverter<SomeData>
    {
        public override SomeData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                SomeData data = SomeData.Struct();
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    string? prop = reader.GetString();
                    if (prop == null)
                        throw new Exception();

                    reader.Read();
                    data[prop] = Read(ref reader, typeToConvert, options);
                    reader.Read();
                }
                return data;
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                SomeData data = SomeData.Array();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    string? prop = reader.GetString();
                    if (prop == null)
                        throw new Exception();

                    reader.Read();
                    data.Items.Add(Read(ref reader, typeToConvert, options));
                    reader.Read();
                }
                return data;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return SomeData.String(reader.GetString());
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return SomeData.Integer(reader.GetInt64());
            }
            else if (reader.TokenType == JsonTokenType.True)
            {
                return SomeData.Boolean(true);
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                return SomeData.Boolean(false);
            }

            return new SomeData();
        }

        public override void Write(Utf8JsonWriter writer, SomeData value, JsonSerializerOptions options)
        {
            if (value.IsScalar)
            {
                writer.WriteRawValue(JsonSerializer.Serialize(value.Value, options));
            }
            else if (value.IsArray)
            {
                WriteArray(writer, value, options);
            }
            else if (value.IsStruct)
            {
                WriteStruct(writer, value, options);
            }
        }

        private void WriteArray(Utf8JsonWriter writer, SomeData value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (SomeData item in value.Items)
                Write(writer, item, options);
            writer.WriteEndArray();
        }

        private void WriteStruct(Utf8JsonWriter writer, SomeData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, SomeData> prop in value.Properties)
            {
                writer.WritePropertyName(prop.Key);
                Write(writer, prop.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
