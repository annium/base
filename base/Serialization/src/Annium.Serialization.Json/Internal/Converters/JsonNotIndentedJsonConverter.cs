using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter that produces compact JSON without indentation for types marked with JsonNotIndentedAttribute.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
internal class JsonNotIndentedJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// Reads and converts JSON to an object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted object.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<T>(
            ref reader,
            options.Clone().RemoveConverter<JsonNotIndentedJsonConverterFactory>()
        )!;
    }

    /// <summary>
    /// Writes a value as compact JSON without indentation.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(
            JsonSerializer.Serialize(
                value,
                options.Clone().NotIndented().RemoveConverter<JsonNotIndentedJsonConverterFactory>()
            )
        );
    }
}
