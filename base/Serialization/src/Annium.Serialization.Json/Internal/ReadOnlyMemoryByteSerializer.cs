using System;
using System.Text;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

/// <summary>
/// JSON serializer implementation that works with ReadOnlyMemory&lt;byte&gt;.
/// </summary>
internal class ReadOnlyMemoryByteSerializer : ISerializer<ReadOnlyMemory<byte>>
{
    /// <summary>
    /// The JSON serializer options to use for serialization and deserialization.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the ReadOnlyMemoryByteSerializer class.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    public ReadOnlyMemoryByteSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Deserializes a ReadOnlyMemory&lt;byte&gt; to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The ReadOnlyMemory&lt;byte&gt; to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(ReadOnlyMemory<byte> value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value.Span, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Deserializes a ReadOnlyMemory&lt;byte&gt; to an object of the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The ReadOnlyMemory&lt;byte&gt; to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, ReadOnlyMemory<byte> value)
    {
        try
        {
            return JsonSerializer.Deserialize(value.Span, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Serializes an object to a ReadOnlyMemory&lt;byte&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized ReadOnlyMemory&lt;byte&gt;.</returns>
    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to serialize {value} as {typeof(T).FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a ReadOnlyMemory&lt;byte&gt;.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized ReadOnlyMemory&lt;byte&gt;.</returns>
    public ReadOnlyMemory<byte> Serialize(Type type, object? value)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
    }
}
