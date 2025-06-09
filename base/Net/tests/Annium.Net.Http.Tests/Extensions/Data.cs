using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Net.Http.Tests.Extensions;

/// <summary>
/// Test data record containing a counter value for HTTP serialization testing
/// </summary>
/// <param name="Counter">The counter value</param>
internal sealed record Data(int Counter);

/// <summary>
/// Custom JSON converter for Data record type
/// </summary>
internal class DataConverter : JsonConverter<Data>
{
    /// <summary>
    /// Reads JSON and converts it to a Data object
    /// </summary>
    /// <param name="reader">The JSON reader</param>
    /// <param name="typeToConvert">The type to convert to</param>
    /// <param name="options">Serializer options</param>
    /// <returns>The deserialized Data object or null if conversion fails</returns>
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

    /// <summary>
    /// Writes a Data object as JSON
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    /// <param name="value">The Data object to serialize</param>
    /// <param name="options">Serializer options</param>
    public override void Write(Utf8JsonWriter writer, Data value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Counter", value.Counter);
        writer.WriteEndObject();
    }
}
