using Efeu.Runtime.Value;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Runtime.Json.Converters
{
    public class EfeuValueJsonConverter : JsonConverter<EfeuValue>
    {
        public override EfeuValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();

                EfeuHash hash = EfeuHash.Empty;
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    string? prop = reader.GetString();
                    if (prop == null)
                        throw new Exception();

                    reader.Read();
                    hash = hash.With(prop, Read(ref reader, typeToConvert, options));
                    reader.Read();
                }
                return hash;
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                EfeuArray array = new EfeuArray();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    array = array.Push(Read(ref reader, typeToConvert, options));
                    reader.Read(); // TODO optimization
                }
                return array;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? EfeuValue.Nil();
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }
            else if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            return EfeuValue.Nil();
        }

        public override void Write(Utf8JsonWriter writer, EfeuValue value, JsonSerializerOptions options)
        {
            if (value.Tag == EfeuValueTag.Integer)
            {
                JsonSerializer.Serialize(writer, value.AsLong(), options);
            }
            else if (value.Tag == EfeuValueTag.True)
            {
                writer.WriteBooleanValue(true);
            }
            else if (value.Tag == EfeuValueTag.False)
            {
                writer.WriteBooleanValue(false);
            }
            else if (value.Tag == EfeuValueTag.Nil)
            {
                writer.WriteNullValue();
            }
            else if (value.Tag == EfeuValueTag.Object)
            {
                if (value.AsObject() is IEnumerable<EfeuValue> array)
                {
                    WriteArray(writer, array, options);
                }
                else if (value.AsObject() is IEnumerable<KeyValuePair<string, EfeuValue>> hash)
                {
                    WriteStruct(writer, hash, options);
                }
                else if (value.AsObject() is EfeuFloat single)
                {
                    writer.WriteNumberValue(single.Value);
                }
                else if (value.AsObject() is EfeuDecimal dec)
                {
                    writer.WriteNumberValue(dec.Value);
                }
                else if (value.AsObject() is EfeuString str)
                {
                    writer.WriteStringValue(str.ToString());
                }
            }
        }

        private void WriteArray(Utf8JsonWriter writer, IEnumerable<EfeuValue> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (EfeuValue item in value)
                Write(writer, item, options);
            writer.WriteEndArray();
        }

        private void WriteStruct(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, EfeuValue>> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, EfeuValue> prop in value)
            {
                writer.WritePropertyName(prop.Key);
                Write(writer, prop.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
