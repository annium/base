using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Internal;

namespace Annium.Serialization.Json;

/// <summary>
/// Delegate for configuring JSON serializer options with access to the service provider.
/// </summary>
/// <param name="provider">The service provider.</param>
/// <param name="options">The JSON serializer options to configure.</param>
public delegate void ConfigureSerializer(IServiceProvider provider, JsonSerializerOptions options);

/// <summary>
/// Extension methods for configuring JSON serialization in the serialization configuration builder.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Cache for configured JsonSerializerOptions instances to avoid recreating them.
    /// </summary>
    private static readonly ConcurrentDictionary<OptionsKey, JsonSerializerOptions> _options = new();

    /// <summary>
    /// Adds JSON serialization support with default configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static void Configure(IServiceProvider sp, JsonSerializerOptions opts) { }

        return builder.WithJson(Configure, isDefault);
    }

    /// <summary>
    /// Adds JSON serialization support with custom configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Action to configure the JSON serializer options.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        Action<JsonSerializerOptions> configure,
        bool isDefault = false
    )
    {
        void Configure(IServiceProvider sp, JsonSerializerOptions opts) => configure(opts);

        return builder.WithJson(Configure, isDefault);
    }

    /// <summary>
    /// Adds JSON serialization support with service provider-aware configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Delegate to configure the JSON serializer options with access to the service provider.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        ConfigureSerializer configure,
        bool isDefault = false
    )
    {
        return builder
            .Register<byte[], ByteArraySerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateByteArray)
            )
            .Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateReadOnlyMemoryByte)
            )
            .Register<string, StringSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateString)
            )
            .Register<Stream, StreamSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateStream)
            );
    }

    /// <summary>
    /// Adds JSON serialization support with pre-configured options.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="opts">The pre-configured JSON serializer options to use.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        JsonSerializerOptions opts,
        bool isDefault = false
    )
    {
        return builder
            .Register<byte[], ByteArraySerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(opts, CreateByteArray)
            )
            .Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(opts, CreateReadOnlyMemoryByte)
            )
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(opts, CreateString))
            .Register<Stream, StreamSerializer>(Constants.MediaType, isDefault, ResolveSerializer(opts, CreateStream));
    }

    /// <summary>
    /// Creates a resolver function for serializers with dynamic configuration.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="key">The serializer key for caching.</param>
    /// <param name="configure">The configuration delegate.</param>
    /// <param name="factory">The factory function to create the serializer.</param>
    /// <returns>A function that resolves the serializer from the service provider.</returns>
    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        ConfigureSerializer configure,
        Func<JsonSerializerOptions, TSerializer> factory
    ) =>
        sp =>
        {
            var optionsKey = new OptionsKey(SerializerKey.Create(key, Constants.MediaType), configure);
            var options = _options.GetOrAdd(
                optionsKey,
                static (key, sp) =>
                {
                    var opts = new JsonSerializerOptions();

                    opts.ConfigureDefault(sp.Resolve<ITypeManager>());

                    key.Configure(sp, opts);
                    return opts;
                },
                sp
            );

            return factory(options);
        };

    /// <summary>
    /// Creates a resolver function for serializers with pre-configured options.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="options">The pre-configured JSON serializer options.</param>
    /// <param name="factory">The factory function to create the serializer.</param>
    /// <returns>A function that resolves the serializer from the service provider.</returns>
    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        JsonSerializerOptions options,
        Func<JsonSerializerOptions, TSerializer> factory
    ) => _ => factory(options);

    /// <summary>
    /// Creates a ByteArraySerializer with the specified options.
    /// </summary>
    /// <param name="opts">The JSON serializer options.</param>
    /// <returns>A new ByteArraySerializer instance.</returns>
    private static ByteArraySerializer CreateByteArray(JsonSerializerOptions opts) => new(opts);

    /// <summary>
    /// Creates a ReadOnlyMemoryByteSerializer with the specified options.
    /// </summary>
    /// <param name="opts">The JSON serializer options.</param>
    /// <returns>A new ReadOnlyMemoryByteSerializer instance.</returns>
    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(JsonSerializerOptions opts) => new(opts);

    /// <summary>
    /// Creates a StringSerializer with the specified options.
    /// </summary>
    /// <param name="opts">The JSON serializer options.</param>
    /// <returns>A new StringSerializer instance.</returns>
    private static StringSerializer CreateString(JsonSerializerOptions opts) => new(opts);

    /// <summary>
    /// Creates a StreamSerializer with the specified options.
    /// </summary>
    /// <param name="opts">The JSON serializer options.</param>
    /// <returns>A new StreamSerializer instance.</returns>
    private static StreamSerializer CreateStream(JsonSerializerOptions opts) => new(opts);

    /// <summary>
    /// Record representing a unique key for caching configured serializer options.
    /// </summary>
    /// <param name="SerializerKey">The serializer key.</param>
    /// <param name="Configure">The configuration delegate.</param>
    private record OptionsKey(SerializerKey SerializerKey, ConfigureSerializer Configure);
}
