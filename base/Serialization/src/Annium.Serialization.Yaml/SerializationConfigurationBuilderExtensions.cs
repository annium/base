using System;
using System.Collections.Concurrent;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Yaml.Internal;
using YamlDotNet.Serialization;
using Constants = Annium.Serialization.Yaml.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring YAML serialization.
/// </summary>
public static class SerializationConfigurationBuilderExtensions
{
    /// <summary>
    /// Cache for YAML serializer and deserializer instances keyed by configuration.
    /// </summary>
    private static readonly ConcurrentDictionary<
        (SerializerKey, Action<IServiceProvider, SerializerBuilder, DeserializerBuilder>),
        (ISerializer, IDeserializer)
    > _options = new();

    /// <summary>
    /// Adds YAML serialization support with default configuration.
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

        return builder.Register<string, StringSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, Configure, CreateString)
        );
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

        return builder.Register<string, StringSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, Configure, CreateString)
        );
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
        Action<IServiceProvider, SerializerBuilder, DeserializerBuilder> configure,
        bool isDefault = false
    )
    {
        return builder.Register<string, StringSerializer>(
            Constants.MediaType,
            isDefault,
            ResolveSerializer(builder.Key, configure, CreateString)
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
        Action<IServiceProvider, SerializerBuilder, DeserializerBuilder> configure,
        Func<ISerializer, IDeserializer, TSerializer> factory
    ) =>
        sp =>
        {
            var (serializer, deserializer) = _options.GetOrAdd(
                (SerializerKey.Create(key, Constants.MediaType), configure),
                _ =>
                {
                    var serializerBuilder = new SerializerBuilder();
                    var deserializerBuilder = new DeserializerBuilder();

                    configure(sp, serializerBuilder, deserializerBuilder);

                    return (serializerBuilder.Build(), deserializerBuilder.Build());
                }
            );

            return factory(serializer, deserializer);
        };

    /// <summary>
    /// Creates a StringSerializer instance with the specified YAML serializer and deserializer.
    /// </summary>
    /// <param name="serializer">The YAML serializer.</param>
    /// <param name="deserializer">The YAML deserializer.</param>
    /// <returns>A new StringSerializer instance.</returns>
    private static StringSerializer CreateString(ISerializer serializer, IDeserializer deserializer) =>
        new(serializer, deserializer);
}
