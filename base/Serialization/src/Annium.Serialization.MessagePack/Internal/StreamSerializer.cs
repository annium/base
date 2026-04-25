using System;
using System.IO;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

/// <summary>
/// A MessagePack serializer implementation that works with streams. Reads/writes are buffered
/// through a <see cref="MemoryStream"/> on the serialization path so the returned stream is
/// positioned at zero and ready to consume.
/// </summary>
internal class StreamSerializer : ISerializer<Stream>
{
    /// <summary>
    /// The MessagePack serializer options used for serialization and deserialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _opts;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamSerializer"/> class.
    /// </summary>
    /// <param name="opts">The MessagePack serializer options to use.</param>
    public StreamSerializer(MessagePackSerializerOptions opts)
    {
        _opts = opts;
    }

    /// <summary>
    /// Deserializes a stream containing MessagePack data into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The stream to deserialize.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/>.</returns>
    public T Deserialize<T>(Stream value)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(value, _opts);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException(
                $"Failed to deserialize stream as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Deserializes a stream containing MessagePack data into the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime type to deserialize to.</param>
    /// <param name="value">The stream to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, Stream value)
    {
        try
        {
            return MessagePackSerializer.Deserialize(type, value, _opts);
        }
        catch (Exception e)
        {
            throw new MessagePackSerializationException($"Failed to deserialize stream as {type.FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a MessagePack stream positioned at zero.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A <see cref="MemoryStream"/> positioned at zero containing the MessagePack payload.</returns>
    public Stream Serialize<T>(T value)
    {
        try
        {
            var ms = new MemoryStream();
            MessagePackSerializer.Serialize(ms, value, _opts);
            ms.Position = 0;
            return ms;
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
    /// Serializes an object of the specified runtime type to a MessagePack stream.
    /// </summary>
    /// <param name="type">The runtime type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A <see cref="MemoryStream"/> positioned at zero containing the MessagePack payload.</returns>
    public Stream Serialize(Type type, object? value)
    {
        try
        {
            var ms = new MemoryStream();
            MessagePackSerializer.Serialize(type, ms, value, _opts);
            ms.Position = 0;
            return ms;
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
