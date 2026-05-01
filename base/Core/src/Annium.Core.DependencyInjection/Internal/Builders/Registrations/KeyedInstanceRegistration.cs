using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for keyed instance-based services
/// </summary>
internal class KeyedInstanceRegistration : IRegistration
{
    /// <summary>
    /// The service type for this registration
    /// </summary>
    private readonly Type _serviceType;

    /// <summary>
    /// The key associated with this service
    /// </summary>
    private readonly object _key;

    /// <summary>
    /// The service instance
    /// </summary>
    private readonly object _instance;

    /// <summary>
    /// Initializes a new instance of the KeyedInstanceRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="instance">The service instance</param>
    public KeyedInstanceRegistration(Type serviceType, object key, object instance)
    {
        _serviceType = serviceType;
        _key = key;
        _instance = instance;
    }

    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        // Mirror non-keyed InstanceRegistration: produce a true keyed-instance descriptor.
        // The previous factory-wrapped path turned an instance into a keyed-factory descriptor,
        // which made the IKeyedInstanceServiceDescriptor branch of ServiceContainer.Contains
        // unreachable for builder-created keyed instances (every re-registration was treated
        // as new and inserted as a duplicate).
        yield return ServiceDescriptor.KeyedInstance(_serviceType, _key, _instance, lifetime);
    }
}
