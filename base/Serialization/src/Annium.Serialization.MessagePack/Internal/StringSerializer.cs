using System;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

/// <summary>
/// A MessagePack serializer implementation that works with strings via base64 encoding of the
/// underlying binary payload. Useful when a textual transport is required for what is natively
/// a binary format.
/// </summary>
internal class StringSerializer : ISerializer<string>
{
    /// <summary>
    /// The MessagePack serializer options used for serialization and deserialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSerializer"/> class.
    /// </summary>
    /// <param name="options">The MessagePack serializer options to use.</param>
    public StringSerializer(MessagePackSerializerOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Deserializes a base64-encoded MessagePack payload into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The base64-encoded string to deserialize.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T Deserialize<T>(string value)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(Convert.FromBase64String(value), _options);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to deserialize {value} as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Deserializes a base64-encoded MessagePack payload into the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime type to deserialize to.</param>
    /// <param name="value">The base64-encoded string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, string value)
    {
        try
        {
            return MessagePackSerializer.Deserialize(type, Convert.FromBase64String(value), _options);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException($"Failed to deserialize {value} as {type.FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a base64-encoded MessagePack string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The base64-encoded MessagePack representation of <paramref name="value"/>.</returns>
    public string Serialize<T>(T value)
    {
        try
        {
            return Convert.ToBase64String(MessagePackSerializer.Serialize(value, _options));
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to serialize {value} as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Serializes an object of the specified runtime type to a base64-encoded MessagePack string.
    /// </summary>
    /// <param name="type">The runtime type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The base64-encoded MessagePack representation of <paramref name="value"/>.</returns>
    public string Serialize(Type type, object? value)
    {
        try
        {
            return Convert.ToBase64String(MessagePackSerializer.Serialize(type, value, _options));
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
    }
}
