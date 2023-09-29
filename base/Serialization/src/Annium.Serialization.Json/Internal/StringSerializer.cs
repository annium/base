using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal;

internal class StringSerializer : ISerializer<string>
{
    private readonly JsonSerializerOptions _options;

    public StringSerializer(
        JsonSerializerOptions options
    )
    {
        _options = options;
    }

    public T Deserialize<T>(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {value} as {typeof(T).FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, string value)
    {
        try
        {
            return JsonSerializer.Deserialize(value, type, _options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"Failed to deserialize {value} as {type.FriendlyName()}", e.Path, e.LineNumber, e.BytePositionInLine, e);
        }
        catch (Exception e)
        {
            throw new JsonException($"Failed to deserialize {value} as {type.FriendlyName()}", e);
        }
    }

    public string Serialize<T>(T value)
    {
        try
        {
            return JsonSerializer.Serialize(value, _options);
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

    public string Serialize(object? value)
    {
        try
        {
            return JsonSerializer.Serialize(value, _options);
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