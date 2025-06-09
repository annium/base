using System;
using Annium.Serialization.Abstractions.Internal;

namespace Annium.Serialization.Abstractions;

/// <summary>
/// Factory class for creating serializer instances.
/// </summary>
public static class Serializer
{
    /// <summary>
    /// Creates a type-aware serializer instance using the provided serialize and deserialize functions.
    /// </summary>
    /// <typeparam name="TValue">The serialization format type.</typeparam>
    /// <param name="serialize">Function to serialize an object of any type to the serialization format.</param>
    /// <param name="deserialize">Function to deserialize from the serialization format to an object of the specified type.</param>
    /// <returns>A new serializer instance.</returns>
    public static ISerializer<TValue> Create<TValue>(
        Func<Type, object?, TValue> serialize,
        Func<Type, TValue, object?> deserialize
    ) => new SerializerInstance<TValue>(serialize, deserialize);

    /// <summary>
    /// Creates a serializer instance for converting between two specific types using the provided serialize and deserialize functions.
    /// </summary>
    /// <typeparam name="TSource">The source type to serialize from.</typeparam>
    /// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
    /// <param name="serialize">Function to serialize from source to destination type.</param>
    /// <param name="deserialize">Function to deserialize from destination to source type.</param>
    /// <returns>A new serializer instance.</returns>
    public static ISerializer<TSource, TDestination> Create<TSource, TDestination>(
        Func<TSource, TDestination> serialize,
        Func<TDestination, TSource> deserialize
    ) => new SerializerInstance<TSource, TDestination>(serialize, deserialize);
}
