using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

/// <summary>
/// JSON serializer implementation that works with strings.
/// </summary>
internal class StringSerializer : ISerializer<string>
{
    /// <summary>
    /// The JSON serializer options to use for serialization and deserialization.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the StringSerializer class.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    public StringSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Deserializes a string to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {value} as {typeof(T).FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Deserializes a string to an object of the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, string value)
    {
        try
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException(
                $"Failed to deserialize {value} as {type.FriendlyName()}",
                e.Path,
                e.LineNumber,
                e.BytePositionInLine,
                e
            );
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {value} as {type.FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized string.</returns>
    public string Serialize<T>(T value)
    {
        try
        {
            return JsonSerializer.Serialize(value, _options);
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
    /// Serializes an object to a string.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized string.</returns>
    public string Serialize(Type type, object? value)
    {
        try
        {
            return JsonSerializer.Serialize(value, type, _options);
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
