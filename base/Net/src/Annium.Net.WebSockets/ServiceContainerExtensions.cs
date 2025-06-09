using Annium.Net.WebSockets;
using Annium.Net.WebSockets.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring WebSocket services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the default connection monitor factory for WebSocket connections
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The configured service container for method chaining</returns>
    public static IServiceContainer AddWebSocketsDefaultConnectionMonitorFactory(this IServiceContainer container)
    {
        container.Add<IConnectionMonitorFactory, DefaultConnectionMonitorFactory>().Singleton();

        return container;
    }
}
