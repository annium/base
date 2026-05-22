using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Logging.Shared;

namespace Annium.Configuration.Tests.Lib;

/// <summary>
/// Builds a minimal <see cref="ServiceContainer"/> with the registrations
/// <c>AddConfigurationAsync</c> needs (runtime types + time + logging + mapper).
/// </summary>
public static class TestContainerFactory
{
    /// <summary>
    /// Creates a service container with runtime / time / logging / mapper registrations
    /// suitable for configuration tests. The runtime scans the calling test assembly.
    /// </summary>
    public static ServiceContainer Create()
    {
        var container = new ServiceContainer();
        container.AddRuntime(Assembly.GetCallingAssembly());
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper(autoload: false);
        return container;
    }
}
