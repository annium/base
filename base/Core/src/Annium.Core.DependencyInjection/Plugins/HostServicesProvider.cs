using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
