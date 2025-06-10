using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for factory-based services
/// </summary>
internal class FactoryRegistration : IRegistration
{
    /// <summary>
    /// The service type for this registration
    /// </summary>
    private readonly Type _serviceType;

    /// <summary>
    /// The factory function to create service instances
    /// </summary>
    private readonly Func<IServiceProvider, object> _factory;

    /// <summary>
    /// Initializes a new instance of the FactoryRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="factory">The factory function</param>
    public FactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory)
    {
        _serviceType = serviceType;
        _factory = factory;
    }

    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Factory(_serviceType, _factory, lifetime);
    }
}
