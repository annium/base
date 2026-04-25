using System;
using System.IO;
using System.Text;
using Annium.Serialization.Abstractions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Annium.Serialization.Yaml.Internal;

/// <summary>
/// A YAML serializer implementation that exchanges <see cref="Stream"/> instances. The stream
/// content is the UTF-8 encoding of the underlying YAML text. Returned streams are positioned
/// at zero and ready to consume.
/// </summary>
internal class StreamSerializer : ISerializer<Stream>
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
    /// Initializes a new instance of the <see cref="StreamSerializer"/> class.
    /// </summary>
    /// <param name="serializer">The YAML serializer to use.</param>
    /// <param name="deserializer">The YAML deserializer to use.</param>
    public StreamSerializer(ISerializer serializer, IDeserializer deserializer)
    {
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Deserializes a stream of UTF-8 YAML text into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The stream to read YAML text from.</param>
    /// <returns>The deserialized object.</returns>
    public T Deserialize<T>(Stream value)
    {
        var text = ReadAllText(value);
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
    /// Deserializes a stream of UTF-8 YAML text into the specified runtime type.
    /// </summary>
    /// <param name="type">The runtime type to deserialize to.</param>
    /// <param name="value">The stream to read YAML text from.</param>
    /// <returns>The deserialized object.</returns>
    public object? Deserialize(Type type, Stream value)
    {
        var text = ReadAllText(value);
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
    /// Serializes an object to a stream of UTF-8 YAML text.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A <see cref="MemoryStream"/> positioned at zero containing UTF-8 YAML text.</returns>
    public Stream Serialize<T>(T value)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(value!));
            return new MemoryStream(bytes);
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
    /// Serializes an object of the specified runtime type to a stream of UTF-8 YAML text.
    /// </summary>
    /// <param name="type">The runtime type of the object to serialize.</param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A <see cref="MemoryStream"/> positioned at zero containing UTF-8 YAML text.</returns>
    public Stream Serialize(Type type, object? value)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(_serializer.Serialize(value, type));
            return new MemoryStream(bytes);
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

    /// <summary>
    /// Reads the supplied stream to the end and decodes it as UTF-8 text.
    /// </summary>
    /// <param name="stream">The stream to read.</param>
    /// <returns>The UTF-8 text content of the stream.</returns>
    private static string ReadAllText(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
