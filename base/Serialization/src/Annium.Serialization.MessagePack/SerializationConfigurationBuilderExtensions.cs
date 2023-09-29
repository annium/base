using System;
using Annium.Serialization.Abstractions;
using Annium.Serialization.MessagePack.Internal;
using Constants = Annium.Serialization.MessagePack.Constants;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class SerializationConfigurationBuilderExtensions
{
    public static ISerializationConfigurationBuilder WithMessagePack(
        this ISerializationConfigurationBuilder builder,
        bool isDefault = false
    ) => builder
        .Register<ReadOnlyMemory<byte>, MessagePackSerializer>(Constants.MediaType, isDefault);
}