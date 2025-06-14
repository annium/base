using Annium.Core.DependencyInjection;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for configuring socket services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds the default connection monitor factory to the service container
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The configured service container</returns>
    public static IServiceContainer AddSocketsDefaultConnectionMonitorFactory(this IServiceContainer container)
    {
        container.Add<IConnectionMonitorFactory, DefaultConnectionMonitorFactory>().Singleton();

        return container;
    }
}
