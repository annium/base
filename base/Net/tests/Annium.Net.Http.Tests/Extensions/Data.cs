using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Net.Http.Tests.Extensions;

internal sealed record Data(int Counter);

internal class DataConverter : JsonConverter<Data>
{
    public override Data? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return null;
        }

        var currentDepth = reader.CurrentDepth;
        var canConvert = false;

        var counter = int.MinValue;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                return canConvert ? new Data(counter) : null;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "Counter":
                        counter = reader.GetInt32();
                        canConvert = true;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of json");
    }

    public override void Write(Utf8JsonWriter writer, Data value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Counter", value.Counter);
        writer.WriteEndObject();
    }
}