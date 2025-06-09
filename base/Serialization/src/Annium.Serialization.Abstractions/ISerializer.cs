using System;

namespace Annium.Serialization.Abstractions;

/// <summary>
/// Defines a serializer for converting between two specific types
/// </summary>
/// <typeparam name="TSource">The source type</typeparam>
/// <typeparam name="TDestination">The destination type</typeparam>
public interface ISerializer<TSource, TDestination>
{
    /// <summary>
    /// Deserializes a value from the destination type to the source type
    /// </summary>
    /// <param name="value">The value to deserialize</param>
    /// <returns>The deserialized value</returns>
    TSource Deserialize(TDestination value);

    /// <summary>
    /// Serializes a value from the source type to the destination type
    /// </summary>
    /// <param name="value">The value to serialize</param>
    /// <returns>The serialized value</returns>
    TDestination Serialize(TSource value);
}

/// <summary>
/// Defines a generic serializer for converting objects to and from a specific representation
/// </summary>
/// <typeparam name="TValue">The serialized representation type</typeparam>
public interface ISerializer<TValue>
{
    /// <summary>
    /// Deserializes a value to the specified type
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <param name="value">The value to deserialize</param>
    /// <returns>The deserialized value</returns>
    T Deserialize<T>(TValue value);

    /// <summary>
    /// Deserializes a value to the specified type
    /// </summary>
    /// <param name="type">The target type</param>
    /// <param name="value">The value to deserialize</param>
    /// <returns>The deserialized value</returns>
    object? Deserialize(Type type, TValue value);

    /// <summary>
    /// Serializes a value of the specified type
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <param name="value">The value to serialize</param>
    /// <returns>The serialized value</returns>
    TValue Serialize<T>(T value);

    /// <summary>
    /// Serializes a value of the specified type
    /// </summary>
    /// <param name="type">The source type</param>
    /// <param name="value">The value to serialize</param>
    /// <returns>The serialized value</returns>
    TValue Serialize(Type type, object? value);
}
