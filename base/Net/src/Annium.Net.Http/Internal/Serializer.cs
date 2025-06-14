using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Net.Http.Internal;

/// <summary>
/// Provides serialization and deserialization functionality for HTTP content based on media types
/// </summary>
internal class Serializer
{
    /// <summary>
    /// Cache for serializer instances to avoid repeated service resolution
    /// </summary>
    private readonly ConcurrentDictionary<string, ISerializer<string>> _cache = new();

    /// <summary>
    /// Service provider for resolving serializer instances
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The key used for serializer resolution
    /// </summary>
    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of the Serializer class
    /// </summary>
    /// <param name="sp">The service provider for resolving serializers</param>
    /// <param name="key">The key used for serializer resolution</param>
    public Serializer(IServiceProvider sp, string key)
    {
        _sp = sp;
        _key = key;
    }

    /// <summary>
    /// Serializes an object to string using the appropriate serializer for the specified media type
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <param name="mediaType">The media type to determine which serializer to use</param>
    /// <param name="value">The object to serialize</param>
    /// <returns>The serialized string representation of the object</returns>
    public string Serialize<T>(string mediaType, T value)
    {
        var serializer = GetSerializer(mediaType);

        return serializer.Serialize(value);
    }

    /// <summary>
    /// Deserializes a string to an object using the appropriate serializer for the specified media type
    /// </summary>
    /// <typeparam name="T">The type to deserialize the string into</typeparam>
    /// <param name="mediaType">The media type to determine which serializer to use</param>
    /// <param name="value">The string value to deserialize</param>
    /// <returns>The deserialized object of type T</returns>
    public T Deserialize<T>(string mediaType, string value)
    {
        var serializer = GetSerializer(mediaType);

        return serializer.Deserialize<T>(value);
    }

    /// <summary>
    /// Gets a serializer for the specified media type, using cache when available
    /// </summary>
    /// <param name="mediaType">The media type to get a serializer for</param>
    /// <returns>The serializer instance for the media type</returns>
    private ISerializer<string> GetSerializer(string mediaType)
    {
        return _cache.GetOrAdd(mediaType, ResolveSerializer);
    }

    /// <summary>
    /// Resolves a serializer instance from the service provider for the specified media type
    /// </summary>
    /// <param name="mediaType">The media type to resolve a serializer for</param>
    /// <returns>The serializer instance for the media type</returns>
    /// <exception cref="NotSupportedException">Thrown when no serializer is found for the media type</exception>
    private ISerializer<string> ResolveSerializer(string mediaType)
    {
        var serializerKey = SerializerKey.Create(_key, mediaType);
        var serializer = _sp.ResolveKeyed<ISerializer<string>>(serializerKey);

        if (serializer is null)
            throw new NotSupportedException($"Media type '{mediaType}' is not supported");

        return serializer;
    }
}
