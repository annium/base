using System;
using Annium.Serialization.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Annium.Serialization.Yaml.Internal;

/// <summary>
/// A YAML serializer implementation that works with string data.
/// </summary>
internal class StringSerializer : ISerializer<string>
{
    /// <summary>
    /// The YAML serializer used for serialization operations.
    /// </summary>
    private readonly ISerializer _serializer;

    /// <summary>
    /// The YAML deserializer used for deserialization operations.
    /// </summary>
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// Initializes a new instance of the StringSerializer class.
    /// </summary>
    /// <param name="serializer">The YAML serializer to use.</param>
    /// <param name="deserializer">The YAML deserializer to use.</param>
    public StringSerializer(ISerializer serializer, IDeserializer deserializer)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Deserializes a YAML string to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The YAML string data to deserialize.</param>
    /// <returns>The deserialized object of type T.</returns>
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

    /// <summary>
    /// Deserializes a YAML string to the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The YAML string data to deserialize.</param>
    /// <returns>The deserialized object of the specified type.</returns>
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

    /// <summary>
    /// Serializes an object to YAML string format.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized data as a YAML string.</returns>
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

    /// <summary>
    /// Serializes an object to YAML string format.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized data as a YAML string.</returns>
    public string Serialize(Type type, object? value)
    {
        try
        {
            return _serializer.Serialize(value, type);
        }
        catch (YamlException e)
        {
            throw new YamlException(
                e.Start,
                e.End,
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
        catch (Exception e)
        {
            throw new YamlException(
                $"Failed to serialize {value} as {value?.GetType().FriendlyName() ?? (object)"null"}",
                e
            );
        }
    }
}
