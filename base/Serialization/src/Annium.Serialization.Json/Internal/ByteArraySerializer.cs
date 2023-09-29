using System;
using System.Text;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

internal class ByteArraySerializer : ISerializer<byte[]>
{
    private readonly JsonSerializerOptions _options;

    public ByteArraySerializer(
        JsonSerializerOptions options
    )
    {
        _options = options;
    }

    public T Deserialize<T>(byte[] value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value)} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value)} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, byte[] value)
    {
        try
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value)} as {type.FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value)} as {type.FriendlyName()}", e);
        }
    }

    public byte[] Serialize<T>(T value)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public byte[] Serialize(object? value)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
    }
}