using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Serialization.Json.Internal.Converters;

internal class GenericDictionaryJsonConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
    where TKey : notnull
    where TValue : notnull
{
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

    public override void Write(
        Utf8JsonWriter writer,
        Dictionary<TKey, TValue> value,
        JsonSerializerOptions options
    )
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