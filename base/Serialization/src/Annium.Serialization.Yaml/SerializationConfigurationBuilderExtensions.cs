using System;
using System.Collections.Concurrent;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Yaml.Internal;
using YamlDotNet.Serialization;
using Constants = Annium.Serialization.Yaml.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class SerializationConfigurationBuilderExtensions
{
    private static readonly ConcurrentDictionary<(SerializerKey, Action<IServiceProvider, SerializerBuilder, DeserializerBuilder>), (ISerializer, IDeserializer)> Options = new();

    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static void Configure(IServiceProvider sp, SerializerBuilder serializer, DeserializerBuilder deserializer)
        {
        }

        return builder
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateString));
    }

    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        Action<SerializerBuilder, DeserializerBuilder> configure,
        bool isDefault = false
    )
    {
        void Configure(IServiceProvider sp, SerializerBuilder serializer, DeserializerBuilder deserializer) => configure(serializer, deserializer);

        return builder
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateString));
    }

    public static ISerializationConfigurationBuilder WithYaml(
        this ISerializationConfigurationBuilder builder,
        Action<IServiceProvider, SerializerBuilder, DeserializerBuilder> configure,
        bool isDefault = false
    )
    {
        return builder
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, configure, CreateString));
    }

    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        Action<IServiceProvider, SerializerBuilder, DeserializerBuilder> configure,
        Func<ISerializer, IDeserializer, TSerializer> factory
    ) => sp =>
    {
        var (serializer, deserializer) = Options.GetOrAdd((SerializerKey.Create(key, Constants.MediaType), configure), _ =>
        {
            var serializerBuilder = new SerializerBuilder();
            var deserializerBuilder = new DeserializerBuilder();

            configure(sp, serializerBuilder, deserializerBuilder);

            return (serializerBuilder.Build(), deserializerBuilder.Build());
        });

        return factory(serializer, deserializer);
    };

    private static StringSerializer CreateString(ISerializer serializer, IDeserializer deserializer) => new(serializer, deserializer);
}