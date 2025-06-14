using System;
using Annium.Core.DependencyInjection;

namespace Annium.Serialization.Abstractions.Internal;

/// <summary>
/// Internal implementation of <see cref="ISerializationConfigurationBuilder"/> for configuring serializers.
/// </summary>
internal class SerializationConfigurationBuilder : ISerializationConfigurationBuilder
{
    /// <summary>
    /// Gets the key associated with this configuration builder.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The service container used to register serializers.
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationConfigurationBuilder"/> class.
    /// </summary>
    /// <param name="container">The service container to register serializers in.</param>
    /// <param name="key">The key to associate with registered serializers.</param>
    public SerializationConfigurationBuilder(IServiceContainer container, string key)
    {
        _container = container;
        Key = key;
    }

    /// <summary>
    /// Registers a serializer for the specified value type and media type.
    /// </summary>
    /// <typeparam name="TValue">The type of values to serialize.</typeparam>
    /// <typeparam name="TSerializer">The serializer implementation type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public ISerializationConfigurationBuilder Register<TValue, TSerializer>(string mediaType, bool isDefault)
        where TSerializer : class, ISerializer<TValue> =>
        RegisterInternal<ISerializer<TValue>, TSerializer>(mediaType, isDefault);

    /// <summary>
    /// Registers a serializer for the specified value type and media type using a custom resolver function.
    /// </summary>
    /// <typeparam name="TValue">The type of values to serialize.</typeparam>
    /// <typeparam name="TSerializer">The serializer implementation type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <param name="resolveSerializer">Function to resolve the serializer instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public ISerializationConfigurationBuilder Register<TValue, TSerializer>(
        string mediaType,
        bool isDefault,
        Func<IServiceProvider, TSerializer> resolveSerializer
    )
        where TSerializer : class, ISerializer<TValue> =>
        RegisterInternal<ISerializer<TValue>, TSerializer>(mediaType, isDefault, resolveSerializer);

    /// <summary>
    /// Registers a serializer for the specified source and destination types.
    /// </summary>
    /// <typeparam name="TSource">The source type to serialize from.</typeparam>
    /// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
    /// <typeparam name="TSerializer">The serializer implementation type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(
        string mediaType,
        bool isDefault
    )
        where TSerializer : class, ISerializer<TSource, TDestination> =>
        RegisterInternal<ISerializer<TSource, TDestination>, TSerializer>(mediaType, isDefault);

    /// <summary>
    /// Registers a serializer for the specified source and destination types using a custom resolver function.
    /// </summary>
    /// <typeparam name="TSource">The source type to serialize from.</typeparam>
    /// <typeparam name="TDestination">The destination type to serialize to.</typeparam>
    /// <typeparam name="TSerializer">The serializer implementation type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <param name="resolveSerializer">Function to resolve the serializer instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public ISerializationConfigurationBuilder Register<TSource, TDestination, TSerializer>(
        string mediaType,
        bool isDefault,
        Func<IServiceProvider, TSerializer> resolveSerializer
    )
        where TSerializer : class, ISerializer<TSource, TDestination> =>
        RegisterInternal<ISerializer<TSource, TDestination>, TSerializer>(mediaType, isDefault, resolveSerializer);

    /// <summary>
    /// Registers a serializer using a custom resolver function.
    /// </summary>
    /// <typeparam name="TISerializer">The serializer interface type.</typeparam>
    /// <typeparam name="TSerializer">The concrete serializer type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <param name="resolveSerializer">Function to resolve the serializer instance.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    private ISerializationConfigurationBuilder RegisterInternal<TISerializer, TSerializer>(
        string mediaType,
        bool isDefault,
        Func<IServiceProvider, TSerializer> resolveSerializer
    )
        where TISerializer : class
        where TSerializer : class, TISerializer
    {
        var key = SerializerKey.Create(Key, mediaType);

        // add serializer via resolver
        _container.Add((sp, _) => resolveSerializer(sp)).AsKeyed<TISerializer>(key).Singleton();

        // register defaults
        if (key.IsDefault)
            RegisterDefault<TISerializer>(key, isDefault);

        return this;
    }

    /// <summary>
    /// Registers a serializer using type registration.
    /// </summary>
    /// <typeparam name="TISerializer">The serializer interface type.</typeparam>
    /// <typeparam name="TSerializer">The concrete serializer type.</typeparam>
    /// <param name="mediaType">The media type this serializer handles.</param>
    /// <param name="isDefault">Whether this should be the default serializer for the media type.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    private ISerializationConfigurationBuilder RegisterInternal<TISerializer, TSerializer>(
        string mediaType,
        bool isDefault
    )
        where TISerializer : class
        where TSerializer : TISerializer
    {
        var key = SerializerKey.Create(Key, mediaType);

        // add serializer via type registration
        _container.Add<TSerializer>().AsKeyed<TISerializer>(key).Singleton();

        // register defaults
        if (key.IsDefault)
            RegisterDefault<TISerializer>(key, isDefault);

        return this;
    }

    /// <summary>
    /// Registers a serializer as the default for its media type if specified.
    /// </summary>
    /// <typeparam name="TISerializer">The serializer interface type.</typeparam>
    /// <param name="key">The serializer key.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    private void RegisterDefault<TISerializer>(SerializerKey key, bool isDefault)
        where TISerializer : class
    {
        // for default key - configure as default for media type
        _container
            .Add(static (sp, key) => sp.ResolveKeyed<TISerializer>(key))
            .AsKeyed<TISerializer>(key.MediaType)
            .Singleton();

        // if default media type - configure as default
        if (isDefault)
            _container.Add(sp => sp.ResolveKeyed<TISerializer>(key)).As<TISerializer>().Singleton();
    }
}
