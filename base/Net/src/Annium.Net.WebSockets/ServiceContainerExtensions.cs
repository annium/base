using Annium.Net.WebSockets;
using Annium.Net.WebSockets.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddDefaultConnectionMonitorFactory(this IServiceContainer container)
    {
        container.Add<IConnectionMonitorFactory, DefaultConnectionMonitorFactory>().Singleton();

        return container;
    }
}
