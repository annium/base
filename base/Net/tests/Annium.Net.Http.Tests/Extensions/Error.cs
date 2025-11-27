using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Net.Http.Tests.Extensions;

/// <summary>
/// Test error record containing failure reason and message for HTTP error testing
/// </summary>
/// <param name="Reason">The HTTP failure reason</param>
/// <param name="Message">The error message</param>
internal sealed record Error(HttpFailureReason Reason, string Message);

/// <summary>
/// Custom JSON converter for Error record type
/// </summary>
internal class ErrorConverter : JsonConverter<Error>
{
    /// <summary>
    /// Reads JSON and converts it to an Error object
    /// </summary>
    /// <param name="reader">The JSON reader</param>
    /// <param name="typeToConvert">The type to convert to</param>
    /// <param name="options">Serializer options</param>
    /// <returns>The deserialized Error object or null if conversion fails</returns>
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
                return canConvert ? new Error(HttpFailureReason.Network, message) : null;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case nameof(Error.Reason):
                        _ = JsonSerializer.Deserialize<HttpFailureReason>(ref reader, options);
                        canConvert = true;
                        break;
                    case nameof(Error.Message):
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

    /// <summary>
    /// Writes an Error object as JSON
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    /// <param name="value">The Error object to serialize</param>
    /// <param name="options">Serializer options</param>
    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Message", value.Message);
        writer.WriteEndObject();
    }
}
