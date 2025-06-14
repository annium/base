using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for instance-based services
/// </summary>
internal class InstanceRegistration : IRegistration
{
    /// <summary>
    /// The service type for this registration
    /// </summary>
    private readonly Type _serviceType;

    /// <summary>
    /// The service instance
    /// </summary>
    private readonly object _instance;

    /// <summary>
    /// Initializes a new instance of the InstanceRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="instance">The service instance</param>
    public InstanceRegistration(Type serviceType, object instance)
    {
        _serviceType = serviceType;
        _instance = instance;
    }

    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Instance(_serviceType, _instance, lifetime);
    }
}
