using System;
using System.Buffers;
using System.Text.Json;

namespace Annium.Serialization.Json;

/// <summary>
/// Extension methods for JsonDocument and JsonElement to provide convenient deserialization functionality.
/// </summary>
public static class JsonElementExtensions
{
    /// <summary>
    /// Deserializes a JsonDocument to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="document">The JsonDocument to deserialize.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(this JsonDocument document, JsonSerializerOptions? options = default)
    {
        var value = document.Deserialize(typeof(T), options);

        return value is null ? default : (T)value;
    }

    /// <summary>
    /// Deserializes a JsonDocument to an object of the specified type.
    /// </summary>
    /// <param name="document">The JsonDocument to deserialize.</param>
    /// <param name="valueType">The type to deserialize to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized object.</returns>
    public static object? Deserialize(
        this JsonDocument document,
        Type valueType,
        JsonSerializerOptions? options = default
    )
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        return document.RootElement.Deserialize(valueType, options);
    }

    /// <summary>
    /// Deserializes a JsonElement to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="element">The JsonElement to deserialize.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized object.</returns>
    public static T? Deserialize<T>(this JsonElement element, JsonSerializerOptions? options = default)
    {
        var value = element.Deserialize(typeof(T), options);

        return value is null ? default : (T)value;
    }

    /// <summary>
    /// Deserializes a JsonElement to an object of the specified type.
    /// </summary>
    /// <param name="element">The JsonElement to deserialize.</param>
    /// <param name="valueType">The type to deserialize to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The deserialized object.</returns>
    public static object? Deserialize(
        this JsonElement element,
        Type valueType,
        JsonSerializerOptions? options = default
    )
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        element.WriteTo(writer);

        writer.Flush();

        return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, valueType, options);
    }
}
