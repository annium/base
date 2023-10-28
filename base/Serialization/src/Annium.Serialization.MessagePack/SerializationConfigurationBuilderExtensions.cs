using System;
using System.Collections.Concurrent;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack.Internal;
using MessagePack;
using Constants = Annium.Serialization.MessagePack.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public delegate MessagePackSerializerOptions ConfigureSerializer(IServiceProvider provider);

public static class SerializationConfigurationBuilderExtensions
{
    private static readonly ConcurrentDictionary<OptionsKey, MessagePackSerializerOptions> Options = new();

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

    private static Func<IServiceProvider, TSerializer> ResolveSerializer<TSerializer>(
        string key,
        ConfigureSerializer configure,
        Func<MessagePackSerializerOptions, TSerializer> factory
    ) =>
        sp =>
        {
            var optionsKey = new OptionsKey(SerializerKey.Create(key, Constants.MediaType), configure);
            var options = Options.GetOrAdd(optionsKey, static (key, sp) => key.Configure(sp), sp);

            return factory(options);
        };

    private static ReadOnlyMemoryByteSerializer CreateReadOnlyMemoryByte(MessagePackSerializerOptions opts) =>
        new(opts);

    //

    private record OptionsKey(SerializerKey SerializerKey, ConfigureSerializer Configure);
}
