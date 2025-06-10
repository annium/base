using System;
using Annium.Core.DependencyInjection.Internal.Packs;
using Annium.Core.DependencyInjection.Packs;

namespace Annium.Core.DependencyInjection.Plugins;

/// <summary>
/// This is emulation class for compatibility with extensions, expecting HostBuilder pattern implementation
/// </summary>
/// <typeparam name="TServicePack"></typeparam>
public class HostServicesBuilder<TServicePack>
    where TServicePack : ServicePackBase, new()
{
    /// <summary>
    /// Builds a host services provider with the specified service pack
    /// </summary>
    /// <returns>The built host services provider</returns>
    public HostServicesProvider Build()
    {
        ServiceProviderBuilder builder = new();
        builder.UseServicePack<TServicePack>();

        return new HostServicesProvider(builder.Build());
    }
}

/// <summary>
/// Provides access to services built by the host services builder
/// </summary>
public class HostServicesProvider
{
    /// <summary>
    /// Gets the service provider containing all registered services
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Initializes a new instance of the HostServicesProvider class
    /// </summary>
    /// <param name="services">The service provider to wrap</param>
    public HostServicesProvider(IServiceProvider services)
    {
        Services = services;
    }
}
