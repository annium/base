using System;
using System.Text;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

/// <summary>
/// A MessagePack serializer implementation that works with <see cref="byte"/> arrays.
/// </summary>
internal class ByteArraySerializer : ISerializer<byte[]>
{
    /// <summary>
    /// The MessagePack serializer options used for serialization and deserialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _opts;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteArraySerializer"/> class.
    /// </summary>
    /// <param name="opts">The MessagePack serializer options to use.</param>
    public ByteArraySerializer(MessagePackSerializerOptions opts)
    {
        _opts = opts;
    }

    /// <summary>
    /// Deserializes a byte array into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The byte array to deserialize.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T Deserialize<T>(byte[] value)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(value, _opts);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value)} as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Deserializes a byte array into the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime type to deserialize to.</param>
    /// <param name="value">The byte array to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, byte[] value)
    {
        try
        {
            return MessagePackSerializer.Deserialize(type, value, _opts);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to deserialize {Encoding.UTF8.GetString(value)} as {type.FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Serializes an object to a byte array.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The MessagePack byte representation of <paramref name="value"/>.</returns>
    public byte[] Serialize<T>(T value)
    {
        try
        {
            return MessagePackSerializer.Serialize(value, _opts);
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
    /// Serializes an object of the specified runtime type to a byte array.
    /// </summary>
    /// <param name="type">The runtime type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The MessagePack byte representation of <paramref name="value"/>.</returns>
    public byte[] Serialize(Type type, object? value)
    {
        try
        {
            return MessagePackSerializer.Serialize(type, value, _opts);
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
