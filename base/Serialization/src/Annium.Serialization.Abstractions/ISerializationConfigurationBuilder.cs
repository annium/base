using System;

namespace Annium.Serialization.Abstractions;

/// <summary>
/// Builder for configuring serialization services
/// </summary>
public interface ISerializationConfigurationBuilder
{
    /// <summary>
    /// Gets the configuration key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Registers a serializer for the specified value type and media type
    /// </summary>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TSerializer">The serializer type</typeparam>
    /// <param name="mediaType">The media type</param>
    /// <param name="isDefault">Whether this is the default serializer</param>
    /// <returns>The configuration builder for method chaining</returns>
    ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TValue>;

    /// <summary>
    /// Registers a serializer with a custom resolver for the specified value type and media type
    /// </summary>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TSerializer">The serializer type</typeparam>
    /// <param name="mediaType">The media type</param>
    /// <param name="isDefault">Whether this is the default serializer</param>
    /// <param name="resolveSerializer">The serializer resolver function</param>
    /// <returns>The configuration builder for method chaining</returns>
    ISerializationConfigurationBuilder Register<TValue, TSerializer>(
        string mediaType,
        bool isDefault,
        Func<IServiceProvider, TSerializer> resolveSerializer
    )
        where TSerializer : class, ISerializer<TValue>;

    /// <summary>
    /// Registers a typed serializer for specific source and destination types
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TDestination">The destination type</typeparam>
    /// <typeparam name="TSerializer">The serializer type</typeparam>
    /// <param name="mediaType">The media type</param>
    /// <param name="isDefault">Whether this is the default serializer</param>
    /// <returns>The configuration builder for method chaining</returns>
    ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TSource, TDestination>;

    /// <summary>
    /// Registers a typed serializer with a custom resolver for specific source and destination types
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TDestination">The destination type</typeparam>
    /// <typeparam name="TSerializer">The serializer type</typeparam>
    /// <param name="mediaType">The media type</param>
    /// <param name="isDefault">Whether this is the default serializer</param>
    /// <param name="resolveSerializer">The serializer resolver function</param>
    /// <returns>The configuration builder for method chaining</returns>
    ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(
        string mediaType,
        bool isDefault,
        Func<IServiceProvider, TSerializer> resolveSerializer
    )
        where TSerializer : class, ISerializer<TSource, TDestination>;
}
