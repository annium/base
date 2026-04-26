using System;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Internal;

/// <summary>
/// Single-arity <see cref="ISerializer{TValue}"/> wrapper exposing the BinaryString package's
/// natural <see cref="string"/> surface. Delegates to the underlying
/// <c>ISerializer&lt;byte[], string&gt;</c> bridge when crossing the byte[]/string boundary.
/// Supported source types are <see cref="string"/> (identity) and <see cref="byte"/>[] (encoded
/// to a hex string via the bridge). Other types throw <see cref="NotSupportedException"/>.
/// </summary>
internal class StringBridgeSerializer : ISerializer<string>
{
    /// <summary>
    /// The underlying byte-array ↔ string bridge.
    /// </summary>
    private readonly ISerializer<byte[], string> _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringBridgeSerializer"/> class.
    /// </summary>
    /// <param name="bridge">The byte-array to string bridge serializer.</param>
    public StringBridgeSerializer(ISerializer<byte[], string> bridge)
    {
        _bridge = bridge;
    }

    /// <summary>
    /// Deserializes a string to <typeparamref name="T"/>. <c>string</c> returns the input
    /// unchanged; <c>byte[]</c> decodes via the bridge; other types are unsupported.
    /// </summary>
    /// <typeparam name="T">The target type — must be <see cref="string"/> or <see cref="byte"/>[].</typeparam>
    /// <param name="value">The string.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="NotSupportedException">When <typeparamref name="T"/> is not string or byte[].</exception>
    public T Deserialize<T>(string value)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;
        if (typeof(T) == typeof(byte[]))
            return (T)(object)_bridge.Deserialize(value);
        throw new NotSupportedException(
            $"BinaryString bridge supports only string and byte[]; T was {typeof(T).FriendlyName()}"
        );
    }

    /// <summary>
    /// Runtime-typed counterpart of <see cref="Deserialize{T}(string)"/>.
    /// </summary>
    /// <param name="type">The target type — must be <see cref="string"/> or <see cref="byte"/>[].</param>
    /// <param name="value">The string.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="NotSupportedException">When <paramref name="type"/> is not string or byte[].</exception>
    public object? Deserialize(Type type, string value)
    {
        if (type == typeof(string))
            return value;
        if (type == typeof(byte[]))
            return _bridge.Deserialize(value);
        throw new NotSupportedException(
            $"BinaryString bridge supports only string and byte[]; type was {type.FriendlyName()}"
        );
    }

    /// <summary>
    /// Serializes a <see cref="string"/> or <see cref="byte"/>[] value to a string.
    /// </summary>
    /// <typeparam name="T">The source type — must be <see cref="string"/> or <see cref="byte"/>[].</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The string representation.</returns>
    /// <exception cref="NotSupportedException">When <typeparamref name="T"/> is not string or byte[].</exception>
    public string Serialize<T>(T value)
    {
        if (value is string text)
            return text;
        if (value is byte[] bytes)
            return _bridge.Serialize(bytes);
        throw new NotSupportedException(
            $"BinaryString bridge supports only string and byte[]; T was {typeof(T).FriendlyName()}"
        );
    }

    /// <summary>
    /// Runtime-typed counterpart of <see cref="Serialize{T}(T)"/>.
    /// </summary>
    /// <param name="type">The source type — must be <see cref="string"/> or <see cref="byte"/>[].</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The string representation.</returns>
    /// <exception cref="NotSupportedException">When <paramref name="type"/> is not string or byte[].</exception>
    public string Serialize(Type type, object? value)
    {
        if (type == typeof(string) && value is string text)
            return text;
        if (type == typeof(byte[]) && value is byte[] bytes)
            return _bridge.Serialize(bytes);
        throw new NotSupportedException(
            $"BinaryString bridge supports only string and byte[]; type was {type.FriendlyName()}"
        );
    }
}
