using Annium.Core.DependencyInjection.Container;
using Annium.Serialization.Abstractions.Internal;

namespace Annium.Serialization.Abstractions;

/// <summary>
/// Extension methods for configuring serialization services in the service container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds serialization configuration to the service container using the default key.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>A configuration builder for registering serializers.</returns>
    public static ISerializationConfigurationBuilder AddSerializers(this IServiceContainer container) =>
        new SerializationConfigurationBuilder(container, Constants.DefaultKey);

    /// <summary>
    /// Adds serialization configuration to the service container using the specified key.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="key">The key to associate with registered serializers.</param>
    /// <returns>A configuration builder for registering serializers.</returns>
    public static ISerializationConfigurationBuilder AddSerializers(this IServiceContainer container, string key) =>
        new SerializationConfigurationBuilder(container, key);
}
