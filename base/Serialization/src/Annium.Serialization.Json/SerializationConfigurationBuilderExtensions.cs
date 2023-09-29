using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json.Internal;
using Constants = Annium.Serialization.Json.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class SerializationConfigurationBuilderExtensions
{
    private static readonly ConcurrentDictionary<(SerializerKey, Action<IServiceProvider, JsonSerializerOptions>), JsonSerializerOptions> Options = new();

    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static void Configure(IServiceProvider sp, JsonSerializerOptions opts)
        {
        }

        return builder
            .Register<byte[], ByteArraySerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateByteArray))
            .Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateReadOnlyMemoryByte))
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateString))
            .Register<Stream, StreamSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateStream));
    }

    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        Action<JsonSerializerOptions> configure,
        bool isDefault = false
    )
    {
        void Configure(IServiceProvider sp, JsonSerializerOptions opts) => configure(opts);

        return builder
            .Register<byte[], ByteArraySerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateByteArray))
            .Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateReadOnlyMemoryByte))
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateString))
            .Register<Stream, StreamSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, Configure, CreateStream));
    }

    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        Action<IServiceProvider, JsonSerializerOptions> configure,
        bool isDefault = false
    )
    {
        return builder
            .Register<byte[], ByteArraySerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, configure, CreateByteArray))
            .Register<ReadOnlyMemory<byte>, ReadOnlyMemoryByteSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, configure, CreateReadOnlyMemoryByte))
            .Register<string, StringSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, configure, CreateString))
            .Register<Stream, StreamSerializer>(Constants.MediaType, isDefault, ResolveSerializer(builder.Key, configure, CreateStream));
    }

    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        Action<IServiceProvider, JsonSerializerOptions> configure,
        Func<JsonSerializerOptions, TSerializer> factory
    ) => sp =>
    {
        var options = Options.GetOrAdd((SerializerKey.Create(key, Constants.MediaType), configure), _ =>
        {
            var opts = new JsonSerializerOptions();

            opts.ConfigureDefault(sp.Resolve<ITypeManager>());

            configure(sp, opts);
            return opts;
        });

        return factory(options);
    };

    private static ByteArraySerializer CreateByteArray(JsonSerializerOptions opts) => new(opts);
    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(JsonSerializerOptions opts) => new(opts);
    private static StringSerializer CreateString(JsonSerializerOptions opts) => new(opts);
    private static StreamSerializer CreateStream(JsonSerializerOptions opts) => new(opts);
}