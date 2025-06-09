using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

/// <summary>
/// JSON serializer implementation that works with streams.
/// </summary>
internal class StreamSerializer : ISerializer<Stream>
{
    /// <summary>
    /// The JSON serializer options to use for serialization and deserialization.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the StreamSerializer class.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    public StreamSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Deserializes a stream to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The stream to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(Stream value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {ToString(value)} as {typeof(T).FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {typeof(T).FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Deserializes a stream to an object of the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The stream to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, Stream value)
    {
        try
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {ToString(value)} as {type.FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {type.FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized stream.</returns>
    public Stream Serialize<T>(T value)
    {
        try
        {
            var ms = new MemoryStream();
            JsonSerializer.Serialize(ms, value, _options);
            ms.Position = 0;
            return ms;
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
    /// Serializes an object to a stream.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized stream.</returns>
    public Stream Serialize(Type type, object? value)
    {
        try
        {
            var ms = new MemoryStream();
            JsonSerializer.Serialize(ms, value, type, _options);
            ms.Position = 0;
            return ms;
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

    /// <summary>
    /// Converts a stream to its string representation for error messages.
    /// </summary>
    /// <param name="value">The stream to convert.</param>
    /// <returns>The string representation of the stream content.</returns>
    private static string ToString(Stream value)
    {
        using var ms = new MemoryStream();
        value.CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
