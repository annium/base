using Annium.Serialization.Abstractions;
using Annium.Serialization.Abstractions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static ISerializationConfigurationBuilder AddSerializers(
        this IServiceContainer container
    ) => new SerializationConfigurationBuilder(container, Constants.DefaultKey);

    public static ISerializationConfigurationBuilder AddSerializers(
        this IServiceContainer container,
        string key
    ) => new SerializationConfigurationBuilder(container, key);
}