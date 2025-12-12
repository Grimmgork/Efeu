using Efeu.Router.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Efeu.Router.JSON.Converters
{
    public class EfeuValueJsonConverter : JsonConverter<EfeuValue>
    {
        public override EfeuValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();

                EfeuHash hash = new EfeuHash(); 
                while (reader.TokenType != JsonTokenType.EndObject)
                {
                    string? prop = reader.GetString();
                    if (prop == null)
                        throw new Exception();

                    reader.Read();
                    hash.Call(prop, Read(ref reader, typeToConvert, options));
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
                    array.Push(Read(ref reader, typeToConvert, options));
                    reader.Read();
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
                JsonSerializer.Serialize(writer, value.ToLong(), options);
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
                if (value.AsObject() is EfeuArray array)
                {
                    WriteArray(writer, array, options);
                }
                else if (value.AsObject() is EfeuHash hash)
                {
                    WriteStruct(writer, hash, options);
                }
                else if (value.AsObject() is EfeuFloat single)
                {
                    writer.WriteNumberValue(single.ToDecimal());
                }
                else if (value.AsObject() is EfeuDecimal dec)
                {
                    writer.WriteNumberValue(dec.ToDecimal());
                }
                else if (value.AsObject() is EfeuString str)
                {
                    writer.WriteStringValue(str.ToString());
                }
            }
        }

        private void WriteArray(Utf8JsonWriter writer, EfeuArray value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (EfeuValue item in value.Each())
                Write(writer, item, options);
            writer.WriteEndArray();
        }

        private void WriteStruct(Utf8JsonWriter writer, EfeuHash value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, EfeuValue> prop in value.Fields())
            {
                writer.WritePropertyName(prop.Key);
                Write(writer, prop.Value, options);
            }
            writer.WriteEndObject();
        }
    }
}
