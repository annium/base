using System;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Internal;

/// <summary>
/// Single-arity <see cref="ISerializer{TValue}"/> wrapper exposing the BinaryString package's
/// natural <see cref="byte"/>[] surface. Delegates to the underlying
/// <c>ISerializer&lt;byte[], string&gt;</c> bridge when crossing the byte[]/string boundary.
/// Supported source types are <see cref="byte"/>[] (identity) and <see cref="string"/> (decoded
/// to bytes via the bridge). Other types throw <see cref="NotSupportedException"/> since
/// BinaryString is an adapter package, not a general object serializer.
/// </summary>
internal class ByteArrayBridgeSerializer : ISerializer<byte[]>
{
    /// <summary>
    /// The underlying byte-array ↔ string bridge.
    /// </summary>
    private readonly ISerializer<byte[], string> _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteArrayBridgeSerializer"/> class.
    /// </summary>
    /// <param name="bridge">The byte-array to string bridge serializer.</param>
    public ByteArrayBridgeSerializer(ISerializer<byte[], string> bridge)
    {
        _bridge = bridge;
    }

    /// <summary>
    /// Deserializes a byte array to <typeparamref name="T"/>. <c>byte[]</c> returns the input
    /// unchanged; <c>string</c> encodes the bytes via the bridge; other types are unsupported.
    /// </summary>
    /// <typeparam name="T">The target type — must be <see cref="byte"/>[] or <see cref="string"/>.</typeparam>
    /// <param name="value">The byte array.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="NotSupportedException">When <typeparamref name="T"/> is not byte[] or string.</exception>
    public T Deserialize<T>(byte[] value)
    {
        if (typeof(T) == typeof(byte[]))
            return (T)(object)value;
        if (typeof(T) == typeof(string))
            return (T)(object)_bridge.Serialize(value);
        throw new NotSupportedException(
            $"BinaryString bridge supports only byte[] and string; T was {typeof(T).FriendlyName()}"
        );
    }

    /// <summary>
    /// Runtime-typed counterpart of <see cref="Deserialize{T}(byte[])"/>.
    /// </summary>
    /// <param name="type">The target type — must be <see cref="byte"/>[] or <see cref="string"/>.</param>
    /// <param name="value">The byte array.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="NotSupportedException">When <paramref name="type"/> is not byte[] or string.</exception>
    public object? Deserialize(Type type, byte[] value)
    {
        if (type == typeof(byte[]))
            return value;
        if (type == typeof(string))
            return _bridge.Serialize(value);
        throw new NotSupportedException(
            $"BinaryString bridge supports only byte[] and string; type was {type.FriendlyName()}"
        );
    }

    /// <summary>
    /// Serializes a <see cref="byte"/>[] or <see cref="string"/> value to a byte array.
    /// </summary>
    /// <typeparam name="T">The source type — must be <see cref="byte"/>[] or <see cref="string"/>.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The byte array representation.</returns>
    /// <exception cref="NotSupportedException">When <typeparamref name="T"/> is not byte[] or string.</exception>
    public byte[] Serialize<T>(T value)
    {
        if (value is byte[] bytes)
            return bytes;
        if (value is string text)
            return _bridge.Deserialize(text);
        throw new NotSupportedException(
            $"BinaryString bridge supports only byte[] and string; T was {typeof(T).FriendlyName()}"
        );
    }

    /// <summary>
    /// Runtime-typed counterpart of <see cref="Serialize{T}(T)"/>.
    /// </summary>
    /// <param name="type">The source type — must be <see cref="byte"/>[] or <see cref="string"/>.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The byte array representation.</returns>
    /// <exception cref="NotSupportedException">When <paramref name="type"/> is not byte[] or string.</exception>
    public byte[] Serialize(Type type, object? value)
    {
        if (type == typeof(byte[]) && value is byte[] bytes)
            return bytes;
        if (type == typeof(string) && value is string text)
            return _bridge.Deserialize(text);
        throw new NotSupportedException(
            $"BinaryString bridge supports only byte[] and string; type was {type.FriendlyName()}"
        );
    }
}
