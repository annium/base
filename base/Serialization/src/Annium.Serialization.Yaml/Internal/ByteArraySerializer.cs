using System;
using System.Text;
using Annium.Serialization.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Annium.Serialization.Yaml.Internal;

/// <summary>
/// A YAML serializer implementation that exchanges <see cref="byte"/> arrays. The byte array
/// is the UTF-8 encoding of the YAML text produced by the underlying string serializer.
/// </summary>
internal class ByteArraySerializer : ISerializer<byte[]>
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
    /// Initializes a new instance of the <see cref="ByteArraySerializer"/> class.
    /// </summary>
    /// <param name="serializer">The YAML serializer to use.</param>
    /// <param name="deserializer">The YAML deserializer to use.</param>
    public ByteArraySerializer(ISerializer serializer, IDeserializer deserializer)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Deserializes a UTF-8 byte array of YAML text into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The byte array containing UTF-8 YAML text.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(byte[] value)
    {
        var text = Encoding.UTF8.GetString(value);
        try
        {
            return _deserializer.Deserialize<T>(text)!;
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to deserialize {text} as {typeof(T).FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to deserialize {text} as {typeof(T).FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Deserializes a UTF-8 byte array of YAML text into the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime type to deserialize to.</param>
    /// <param name="value">The byte array containing UTF-8 YAML text.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, byte[] value)
    {
        var text = Encoding.UTF8.GetString(value);
        try
        {
            return _deserializer.Deserialize(text, type);
        }
        catch (YamlException e)
        {
            throw new YamlException(e.Start, e.End, $"Failed to deserialize {text} as {type.FriendlyName()}", e);
        }
        catch (Exception e)
        {
            throw new YamlException($"Failed to deserialize {text} as {type.FriendlyName()}", e);
        }
    }

    /// <summary>
    /// Serializes an object to a UTF-8 byte array of YAML text.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>UTF-8 bytes of the YAML text representation.</returns>
    public byte[] Serialize<T>(T value)
    {
        try
        {
            return Encoding.UTF8.GetBytes(_serializer.Serialize(value!));
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
    /// Serializes an object of the specified runtime type to a UTF-8 byte array of YAML text.
    /// </summary>
    /// <param name="type">The runtime type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>UTF-8 bytes of the YAML text representation.</returns>
    public byte[] Serialize(Type type, object? value)
    {
        try
        {
            return Encoding.UTF8.GetBytes(_serializer.Serialize(value, type));
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
