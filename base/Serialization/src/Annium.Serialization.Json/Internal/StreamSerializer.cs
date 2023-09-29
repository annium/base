using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

internal class StreamSerializer : ISerializer<Stream>
{
    private readonly JsonSerializerOptions _options;

    public StreamSerializer(
        JsonSerializerOptions options
    )
    {
        _options = options;
    }

    public T Deserialize<T>(Stream value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, Stream value)
    {
        try
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {type.FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {ToString(value)} as {type.FriendlyName()}", e);
        }
    }

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
            throw new JsonException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public Stream Serialize(object? value)
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
            throw new JsonException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
    }

    private static string ToString(Stream value)
    {
        using var ms = new MemoryStream();
        value.CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}