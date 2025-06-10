using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Serialization.Abstractions;

/// <summary>
/// Extension methods for resolving serializers from the service provider.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Resolves a serializer for the specified type and media type using the default key.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="mediaType">The media type of the serializer.</param>
    /// <returns>The resolved serializer.</returns>
    public static ISerializer<T> ResolveSerializer<T>(this IServiceProvider provider, string mediaType)
    {
        return provider.ResolveSerializer<T>(Constants.DefaultKey, mediaType);
    }

    /// <summary>
    /// Resolves a serializer for the specified type, key, and media type.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="key">The key associated with the serializer.</param>
    /// <param name="mediaType">The media type of the serializer.</param>
    /// <returns>The resolved serializer.</returns>
    public static ISerializer<T> ResolveSerializer<T>(this IServiceProvider provider, string key, string mediaType)
    {
        var serializerKey = SerializerKey.Create(key, mediaType);

        return provider.GetRequiredKeyedService<ISerializer<T>>(serializerKey);
    }

    /// <summary>
    /// Resolves a serializer for the specified source and destination types and media type using the default key.
    /// </summary>
    /// <typeparam name="TSource">The source type to serialize from.</typeparam>
    /// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="mediaType">The media type of the serializer.</param>
    /// <returns>The resolved serializer.</returns>
    public static ISerializer<TSource, TDestination> ResolveSerializer<TSource, TDestination>(
        this IServiceProvider provider,
        string mediaType
    )
    {
        return provider.ResolveSerializer<TSource, TDestination>(Constants.DefaultKey, mediaType);
    }

    /// <summary>
    /// Resolves a serializer for the specified source and destination types, key, and media type.
    /// </summary>
    /// <typeparam name="TSource">The source type to serialize from.</typeparam>
    /// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
    /// <param name="provider">The service provider.</param>
    /// <param name="key">The key associated with the serializer.</param>
    /// <param name="mediaType">The media type of the serializer.</param>
    /// <returns>The resolved serializer.</returns>
    public static ISerializer<TSource, TDestination> ResolveSerializer<TSource, TDestination>(
        this IServiceProvider provider,
        string key,
        string mediaType
    )
    {
        var serializerKey = SerializerKey.Create(key, mediaType);

        return provider.GetRequiredKeyedService<ISerializer<TSource, TDestination>>(serializerKey);
    }
}
