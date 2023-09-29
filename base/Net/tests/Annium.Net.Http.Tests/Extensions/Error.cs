using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Net.Http.Tests.Extensions;

internal sealed record Error(string Message);

internal class ErrorConverter : JsonConverter<Error>
{
    public override Error? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return null;
        }

        var currentDepth = reader.CurrentDepth;
        var canConvert = false;

        var message = string.Empty;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                return canConvert ? new Error(message) : null;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "Message":
                        message = reader.GetString() ?? string.Empty;
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

    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Message", value.Message);
        writer.WriteEndObject();
    }
}