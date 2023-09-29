using System;
using System.Text;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

internal class MessagePackSerializer : ISerializer<ReadOnlyMemory<byte>>
{
    private readonly MessagePackSerializerOptions _opts =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public T Deserialize<T>(ReadOnlyMemory<byte> value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Deserialize<T>(value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new Exception($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, ReadOnlyMemory<byte> value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Deserialize(type, value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new Exception($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}", e);
        }
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new MessagePackSerializationException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public ReadOnlyMemory<byte> Serialize(object? value)
    {
        try
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }
        catch (MessagePackSerializationException e)
        {
            throw new MessagePackSerializationException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
    }
}