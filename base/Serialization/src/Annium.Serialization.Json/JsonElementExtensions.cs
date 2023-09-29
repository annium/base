using System;
using System.Buffers;
using System.Text.Json;

namespace Annium.Serialization.Json;

public static class JsonElementExtensions
{
    public static T? Deserialize<T>(this JsonDocument document, JsonSerializerOptions? options = default)
    {
        var value = document.Deserialize(typeof(T), options);

        return value is null ? default : (T)value;
    }

    public static object? Deserialize(this JsonDocument document, Type valueType, JsonSerializerOptions? options = default)
    {
        if (document is null) throw new ArgumentNullException(nameof(document));

        return document.RootElement.Deserialize(valueType, options);
    }

    public static T? Deserialize<T>(this JsonElement element, JsonSerializerOptions? options = default)
    {
        var value = element.Deserialize(typeof(T), options);

        return value is null ? default : (T)value;
    }

    public static object? Deserialize(this JsonElement element, Type valueType, JsonSerializerOptions? options = default)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);
        element.WriteTo(writer);

        writer.Flush();

        return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, valueType, options);
    }
}