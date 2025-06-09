using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter for generic dictionaries that serializes complex keys as JSON strings.
/// </summary>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal class GenericDictionaryJsonConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull
{
    /// <summary>
    /// Reads and converts JSON to a dictionary.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted dictionary.</returns>
    public override Dictionary<TKey, TValue> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        var result = new Dictionary<TKey, TValue>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return result;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            // TODO: try parse with & without quotes
            var keyString = reader.GetString()!;
            keyString = keyString.StartsWith("{") || keyString.StartsWith("[") ? keyString : $@"""{keyString}""";
            var key = JsonSerializer.Deserialize<TKey>(keyString, options)!;
            reader.Read();
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options)!;
            result[key] = value;
        }

        throw new JsonException();
    }

    /// <summary>
    /// Writes a dictionary as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The dictionary to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var pair in value)
        {
            writer.WritePropertyName(JsonSerializer.Serialize(pair.Key, options).Trim('"'));
            JsonSerializer.Serialize(writer, pair.Value, options);
        }

        writer.WriteEndObject();
    }
}
