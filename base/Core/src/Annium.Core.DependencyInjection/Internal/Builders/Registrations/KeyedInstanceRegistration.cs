using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

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
        yield return Factory(_serviceType, _key, (_, _) => Expression.Constant(_instance), lifetime);
    }
}
