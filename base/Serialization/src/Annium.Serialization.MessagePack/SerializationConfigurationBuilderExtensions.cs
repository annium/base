using System;
using System.Collections.Concurrent;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack.Internal;
using MessagePack;

namespace Annium.Serialization.MessagePack;

/// <summary>
/// Delegate for configuring MessagePack serializer options using a service provider.
/// </summary>
/// <param name="provider">The service provider for dependency resolution.</param>
/// <returns>The configured MessagePackSerializerOptions.</returns>
public delegate MessagePackSerializerOptions ConfigureSerializer(IServiceProvider provider);

/// <summary>
/// Extension methods for configuring MessagePack serialization.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Cache for MessagePack serializer options keyed by options configuration.
    /// </summary>
    private static readonly ConcurrentDictionary<OptionsKey, MessagePackSerializerOptions> _options = new();

    /// <summary>
    /// Adds MessagePack serialization support with default configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static MessagePackSerializerOptions Configure(IServiceProvider sp) =>
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        return builder.Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, Configure, CreateReadOnlyMemoryByte)
        );
    }

    /// <summary>
    /// Adds MessagePack serialization support with custom configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Function to configure MessagePack serializer options.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        Func<MessagePackSerializerOptions> configure,
        bool isDefault = false
    )
    {
        MessagePackSerializerOptions Configure(IServiceProvider sp) => configure();

        return builder.Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, Configure, CreateReadOnlyMemoryByte)
        );
    }

    /// <summary>
    /// Adds MessagePack serialization support with service provider-based configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Delegate to configure MessagePack serializer options using service provider.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        ConfigureSerializer configure,
        bool isDefault = false
    )
    {
        return builder.Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, configure, CreateReadOnlyMemoryByte)
        );
    }

    /// <summary>
    /// Creates a function to resolve a serializer instance with configuration.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="key">The serializer key for caching.</param>
    /// <param name="configure">The configuration delegate.</param>
    /// <param name="factory">The factory function to create the serializer.</param>
    /// <returns>A function that resolves the serializer from a service provider.</returns>
    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        ConfigureSerializer configure,
        Func<MessagePackSerializerOptions, TSerializer> factory
    ) =>
        sp =>
        {
            var optionsKey = new OptionsKey(SerializerKey.Create(key, Constants.MediaType), configure);
            var options = _options.GetOrAdd(optionsKey, static (key, sp) => key.Configure(sp), sp);

            return factory(options);
        };

    /// <summary>
    /// Adds MessagePack serialization support with pre-configured options.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="opts">The pre-configured MessagePack serializer options.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        MessagePackSerializerOptions opts,
        bool isDefault = false
    )
    {
        return builder.Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(opts, CreateReadOnlyMemoryByte)
        );
    }

    /// <summary>
    /// Creates a function to resolve a serializer instance with pre-configured options.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="options">The pre-configured MessagePack serializer options.</param>
    /// <param name="factory">The factory function to create the serializer.</param>
    /// <returns>A function that resolves the serializer from a service provider.</returns>
    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        MessagePackSerializerOptions options,
        Func<MessagePackSerializerOptions, TSerializer> factory
    ) => _ => factory(options);

    /// <summary>
    /// Creates a ReadOnlyMemoryByteSerializer instance with the specified options.
    /// </summary>
    /// <param name="opts">The MessagePack serializer options.</param>
    /// <returns>A new ReadOnlyMemoryByteSerializer instance.</returns>
    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(MessagePackSerializerOptions opts) =>
        new(opts);

    //

    /// <summary>
    /// Record representing a cache key for MessagePack serializer options.
    /// </summary>
    /// <param name="SerializerKey">The serializer key identifying the configuration.</param>
    /// <param name="Configure">The configuration delegate used to create options.</param>
    private record OptionsKey(SerializerKey SerializerKey, ConfigureSerializer Configure);
}
