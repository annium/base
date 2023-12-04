using Annium.Net.Sockets;
using Annium.Net.Sockets.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddSocketsDefaultConnectionMonitorFactory(this IServiceContainer container)
    {
        container.Add<IConnectionMonitorFactory, DefaultConnectionMonitorFactory>().Singleton();

        return container;
    }
}
