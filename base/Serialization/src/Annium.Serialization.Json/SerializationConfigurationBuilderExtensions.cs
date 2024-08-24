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

public delegate void ConfigureSerializer(IServiceProvider provider, JsonSerializerOptions options);

public static class SerializationConfigurationBuilderExtensions
{
    private static readonly ConcurrentDictionary<OptionsKey, JsonSerializerOptions> _options = new();

    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    )
    {
        static void Configure(IServiceProvider sp, JsonSerializerOptions opts) { }

        return builder.WithJson(Configure, isDefault);
    }

    public static ISerializationConfigurationBuilder WithJson(
        this ISerializationConfigurationBuilder builder,
        Action<JsonSerializerOptions> configure,
        bool isDefault = false
    )
    {
        void Configure(IServiceProvider sp, JsonSerializerOptions opts) => configure(opts);

        return builder.WithJson(Configure, isDefault);
    }

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

    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        JsonSerializerOptions options,
        Func<JsonSerializerOptions, TSerializer> factory
    ) => _ => factory(options);

    private static ByteArraySerializer CreateByteArray(JsonSerializerOptions opts) => new(opts);

    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(JsonSerializerOptions opts) => new(opts);

    private static StringSerializer CreateString(JsonSerializerOptions opts) => new(opts);

    private static StreamSerializer CreateStream(JsonSerializerOptions opts) => new(opts);

    private record OptionsKey(SerializerKey SerializerKey, ConfigureSerializer Configure);
}
