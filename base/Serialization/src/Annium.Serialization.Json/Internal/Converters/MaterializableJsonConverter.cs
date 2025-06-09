using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Data.Models;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter for materializable types that calls OnMaterialized after deserialization.
/// </summary>
/// <typeparam name="T">The materializable type to convert.</typeparam>
internal class MaterializableJsonConverter<T> : JsonConverter<T>
    where T : IMaterializable
{
    /// <summary>
    /// Reads and converts JSON to a materializable object, calling OnMaterialized after deserialization.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted and materialized object.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value =
            JsonSerializer.Deserialize<T>(
                ref reader,
                options.Clone().RemoveConverter<MaterializableJsonConverterFactory>()
            ) ?? throw new JsonException($"Failed to deserialize {typeof(T).FriendlyName()}");

        value.OnMaterialized();

        return value;
    }

    /// <summary>
    /// Writes a materializable value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The materializable value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<MaterializableJsonConverterFactory>());
    }
}
