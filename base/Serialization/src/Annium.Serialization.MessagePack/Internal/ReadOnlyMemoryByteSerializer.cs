using System;
using System.Text;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal;

internal class ReadOnlyMemoryByteSerializer : ISerializer<ReadOnlyMemory<byte>>
{
    private readonly MessagePackSerializerOptions _opts;

    public ReadOnlyMemoryByteSerializer(MessagePackSerializerOptions opts)
    {
        _opts = opts;
    }

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
