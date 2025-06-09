using System;
using System.Text;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

/// <summary>
/// A MessagePack serializer implementation that works with ReadOnlyMemory&lt;byte&gt; data.
/// </summary>
internal class ReadOnlyMemoryByteSerializer : ISerializer<ReadOnlyMemory<byte>>
{
    /// <summary>
    /// The MessagePack serializer options used for serialization and deserialization.
    /// </summary>
    private readonly MessagePackSerializerOptions _opts;

    /// <summary>
    /// Initializes a new instance of the ReadOnlyMemoryByteSerializer class.
    /// </summary>
    /// <param name="opts">The MessagePack serializer options to use.</param>
    public ReadOnlyMemoryByteSerializer(MessagePackSerializerOptions opts)
    {
        _opts = opts;
    }

    /// <summary>
    /// Deserializes a ReadOnlyMemory&lt;byte&gt; value to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The ReadOnlyMemory&lt;byte&gt; data to deserialize.</param>
    /// <returns>The deserialized object of type T.</returns>
    public T Deserialize<T>(ReadOnlyMemory<byte> value)
    {
        try
        {
            return MessagePackSerializer.Deserialize<T>(value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new Exception(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}",
                e
            );
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Deserializes a ReadOnlyMemory&lt;byte&gt; value to the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The ReadOnlyMemory&lt;byte&gt; data to deserialize.</param>
    /// <returns>The deserialized object of the specified type.</returns>
    public object? Deserialize(Type type, ReadOnlyMemory<byte> value)
    {
        try
        {
            return MessagePackSerializer.Deserialize(type, value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new Exception(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}",
                e
            );
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}",
                e
            );
        }
    }

    /// <summary>
    /// Serializes an object to ReadOnlyMemory&lt;byte&gt; format.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized data as ReadOnlyMemory&lt;byte&gt;.</returns>
    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        try
        {
            return MessagePackSerializer.Serialize(value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new MessagePackSerializationException(
                $"Failed to serialize {value} as {typeof(T).FriendlyName()}",
                e
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to ReadOnlyMemory&lt;byte&gt; format.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized data as ReadOnlyMemory&lt;byte&gt;.</returns>
    public ReadOnlyMemory<byte> Serialize(Type type, object? value)
    {
        try
        {
            return MessagePackSerializer.Serialize(type, value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new MessagePackSerializationException(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
    }
}
