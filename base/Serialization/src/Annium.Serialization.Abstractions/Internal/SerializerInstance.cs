using System;

namespace Annium.Serialization.Abstractions.Internal;

/// <summary>
/// Internal implementation of <see cref="ISerializer{TSource, TDestination}"/> that uses delegates for serialization and deserialization.
/// </summary>
/// <typeparam name="TSource">The source type to serialize from.</typeparam>
/// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
internal class SerializerInstance<TSource, TDestination> : ISerializer<TSource, TDestination>
{
    /// <summary>
    /// Function to serialize from source to destination type.
    /// </summary>
    private readonly Func<TSource, TDestination> _serialize;

    /// <summary>
    /// Function to deserialize from destination to source type.
    /// </summary>
    private readonly Func<TDestination, TSource> _deserialize;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializerInstance{TSource, TDestination}"/> class.
    /// </summary>
    /// <param name="serialize">Function to serialize from source to destination type.</param>
    /// <param name="deserialize">Function to deserialize from destination to source type.</param>
    public SerializerInstance(Func<TSource, TDestination> serialize, Func<TDestination, TSource> deserialize)
    {
        _deserialize = deserialize;
        _serialize = serialize;
    }

    /// <summary>
    /// Deserializes a value from the destination type to the source type.
    /// </summary>
    /// <param name="value">The value to deserialize.</param>
    /// <returns>The deserialized value.</returns>
    public TSource Deserialize(TDestination value) => _deserialize(value);

    /// <summary>
    /// Serializes a value from the source type to the destination type.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized value.</returns>
    public TDestination Serialize(TSource value) => _serialize(value);
}

/// <summary>
/// Internal implementation of <see cref="ISerializer{TValue}"/> that uses delegates for type-aware serialization and deserialization.
/// </summary>
/// <typeparam name="TValue">The serialization format type.</typeparam>
internal class SerializerInstance<TValue> : ISerializer<TValue>
{
    /// <summary>
    /// Function to serialize an object of any type to the serialization format.
    /// </summary>
    private readonly Func<Type, object?, TValue> _serialize;

    /// <summary>
    /// Function to deserialize from the serialization format to an object of the specified type.
    /// </summary>
    private readonly Func<Type, TValue, object?> _deserialize;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializerInstance{TValue}"/> class.
    /// </summary>
    /// <param name="serialize">Function to serialize an object of any type to the serialization format.</param>
    /// <param name="deserialize">Function to deserialize from the serialization format to an object of the specified type.</param>
    public SerializerInstance(Func<Type, object?, TValue> serialize, Func<Type, TValue, object?> deserialize)
    {
        _serialize = serialize;
        _deserialize = deserialize;
    }

    /// <summary>
    /// Deserializes a value to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="value">The value to deserialize.</param>
    /// <returns>The deserialized value.</returns>
    public T Deserialize<T>(TValue value) => (T)_deserialize(typeof(T), value)!;

    /// <summary>
    /// Deserializes a value to the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="value">The value to deserialize.</param>
    /// <returns>The deserialized value.</returns>
    public object? Deserialize(Type type, TValue value) => _deserialize(type, value);

    /// <summary>
    /// Serializes a value of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized value.</returns>
    public TValue Serialize<T>(T value) => _serialize(typeof(T), value);

    /// <summary>
    /// Serializes a value of the specified type.
    /// </summary>
    /// <param name="type">The type of the value to serialize.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized value.</returns>
    public TValue Serialize(Type type, object? value) => _serialize(type, value);
}
