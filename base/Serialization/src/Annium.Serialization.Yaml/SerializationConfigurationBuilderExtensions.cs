using System;
using System.Collections.Concurrent;
using System.IO;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Yaml.Internal;
using YamlDotNet.Serialization;

namespace Annium.Serialization.Yaml;

/// <summary>
/// Delegate for configuring YAML serializer and deserializer builders with access to the service provider.
/// </summary>
/// <param name="provider">The service provider.</param>
/// <param name="serializer">The YAML serializer builder to configure.</param>
/// <param name="deserializer">The YAML deserializer builder to configure.</param>
public delegate void ConfigureSerializer(
    IServiceProvider provider,
    SerializerBuilder serializer,
    DeserializerBuilder deserializer
);

/// <summary>
/// Extension methods for configuring YAML serialization.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Cache for YAML serializer and deserializer instances keyed by configuration.
    /// </summary>
    private static readonly ConcurrentDictionary<OptionsKey, (ISerializer, IDeserializer)> _options = new();

    /// <summary>
    /// Adds YAML serialization support with default configuration. Registers serializers for the
    /// <see cref="string"/>, <see cref="byte"/>[], <see cref="ReadOnlyMemory{T}"/> of <see cref="byte"/>,
    /// and <see cref="Stream"/> surfaces, all backed by UTF-8 encoding of the same YAML text.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static void Configure(IServiceProvider sp, SerializerBuilder serializer, DeserializerBuilder deserializer) { }

        return RegisterAll(builder, isDefault, Configure);
    }

    /// <summary>
    /// Adds YAML serialization support with custom configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Action to configure YAML serializer and deserializer builders.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        Action<SerializerBuilder, DeserializerBuilder> configure,
        bool isDefault = false
    )
    {
        void Configure(IServiceProvider sp, SerializerBuilder serializer, DeserializerBuilder deserializer) =>
            configure(serializer, deserializer);

        return RegisterAll(builder, isDefault, Configure);
    }

    /// <summary>
    /// Adds YAML serialization support with service provider-based configuration.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="configure">Action to configure YAML serializer and deserializer builders using service provider.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        ConfigureSerializer configure,
        bool isDefault = false
    )
    {
        return RegisterAll(builder, isDefault, configure);
    }

    /// <summary>
    /// Registers all four YAML serializer surfaces against the supplied configuration delegate.
    /// </summary>
    /// <param name="builder">The serialization configuration builder.</param>
    /// <param name="isDefault">Whether this should be the default serializer.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    private static ISerializationConfigurationBuilder RegisterAll(
        ISerializationConfigurationBuilder builder,
        bool isDefault,
        ConfigureSerializer configure
    )
    {
        return builder
            .Register<string, StringSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateString)
            )
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
            .Register<Stream, StreamSerializer>(
                Constants.MediaType,
                isDefault,
                ResolveSerializer(builder.Key, configure, CreateStream)
            );
    }

    /// <summary>
    /// Creates a function to resolve a serializer instance with configuration.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="key">The serializer key for caching.</param>
    /// <param name="configure">The configuration action for serializer and deserializer builders.</param>
    /// <param name="factory">The factory function to create the serializer from YAML components.</param>
    /// <returns>A function that resolves the serializer from a service provider.</returns>
    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        ConfigureSerializer configure,
        Func<ISerializer, IDeserializer, TSerializer> factory
    ) =>
        sp =>
        {
            var (serializer, deserializer) = _options.GetOrAdd(
                new OptionsKey(SerializerKey.Create(key, Constants.MediaType), configure),
                static (optionsKey, provider) =>
                {
                    var serializerBuilder = new SerializerBuilder();
                    var deserializerBuilder = new DeserializerBuilder();

                    optionsKey.Configure(provider, serializerBuilder, deserializerBuilder);

                    return (serializerBuilder.Build(), deserializerBuilder.Build());
                },
                sp
            );

            return factory(serializer, deserializer);
        };

    /// <summary>
    /// Creates a <see cref="StringSerializer"/> instance with the specified YAML serializer and deserializer.
    /// </summary>
    /// <param name="serializer">The YAML serializer.</param>
    /// <param name="deserializer">The YAML deserializer.</param>
    /// <returns>A new <see cref="StringSerializer"/> instance.</returns>
    private static StringSerializer CreateString(ISerializer serializer, IDeserializer deserializer) =>
        new(serializer, deserializer);

    /// <summary>
    /// Creates a <see cref="ByteArraySerializer"/> instance.
    /// </summary>
    /// <param name="serializer">The YAML serializer.</param>
    /// <param name="deserializer">The YAML deserializer.</param>
    /// <returns>A new <see cref="ByteArraySerializer"/> instance.</returns>
    private static ByteArraySerializer CreateByteArray(ISerializer serializer, IDeserializer deserializer) =>
        new(serializer, deserializer);

    /// <summary>
    /// Creates a <see cref="ReadOnlyMemoryByteSerializer"/> instance.
    /// </summary>
    /// <param name="serializer">The YAML serializer.</param>
    /// <param name="deserializer">The YAML deserializer.</param>
    /// <returns>A new <see cref="ReadOnlyMemoryByteSerializer"/> instance.</returns>
    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(
        ISerializer serializer,
        IDeserializer deserializer
    ) => new(serializer, deserializer);

    /// <summary>
    /// Creates a <see cref="StreamSerializer"/> instance.
    /// </summary>
    /// <param name="serializer">The YAML serializer.</param>
    /// <param name="deserializer">The YAML deserializer.</param>
    /// <returns>A new <see cref="StreamSerializer"/> instance.</returns>
    private static StreamSerializer CreateStream(ISerializer serializer, IDeserializer deserializer) =>
        new(serializer, deserializer);

    /// <summary>
    /// Record representing a unique key for caching configured YAML serializer/deserializer pairs.
    /// </summary>
    /// <param name="SerializerKey">The serializer key.</param>
    /// <param name="Configure">The configuration delegate.</param>
    private record OptionsKey(SerializerKey SerializerKey, ConfigureSerializer Configure);
}
