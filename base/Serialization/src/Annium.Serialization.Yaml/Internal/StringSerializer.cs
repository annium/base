using System;
using Annium.Serialization.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Annium.Serialization.Yaml.Internal;

internal class StringSerializer : ISerializer<string>
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public StringSerializer(
        ISerializer serializer,
        IDeserializer deserializer
    )
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    public T Deserialize<T>(string value)
    {
        try
        {
            return _deserializer.Deserialize<T>(value)!;
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to deserialize {value} as {typeof(T).FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to deserialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public object? Deserialize(Type type, string value)
    {
        try
        {
            return _deserializer.Deserialize(value, type);
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to deserialize {value} as {type.FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to deserialize {value} as {type.FriendlyName()}", e);
        }
    }

    public string Serialize<T>(T value)
    {
        try
        {
            return _serializer.Serialize(value!);
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to serialize {value} as {typeof(T).FriendlyName()}", e);
        }
    }

    public string Serialize(object? value)
    {
        try
        {
            return _serializer.Serialize(value!);
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}", e);
        }
    }
}