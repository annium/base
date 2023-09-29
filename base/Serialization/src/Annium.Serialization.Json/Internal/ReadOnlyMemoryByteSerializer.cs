using System;
using System.Text;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

internal class ReadOnlyMemoryByteSerializer : ISerializer<ReadOnlyMemory<byte>>
{
    private readonly JsonSerializerOptions _options;

    public ReadOnlyMemoryByteSerializer(
        JsonSerializerOptions options
    )
    {
        _options = options;
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value.Span, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, ReadOnlyMemory<byte> value)
    {
        try
        {
            return JsonSerializer.Deserialize(value.Span, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {Encoding.UTF8.GetString(value.ToArray())} as {type.FriendlyName()}", e);
        }
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value)
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

    public ReadOnlyMemory<byte> Serialize(object? value)
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