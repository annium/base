using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Descriptors;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for keyed type-based services
/// </summary>
internal class KeyedTypeRegistration : IRegistration
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
    /// The implementation type for this service
    /// </summary>
    private readonly Type _implementationType;

    /// <summary>
    /// Initializes a new instance of the KeyedTypeRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    public KeyedTypeRegistration(Type serviceType, object key, Type implementationType)
    {
        _serviceType = serviceType;
        _key = key;
        _implementationType = implementationType;
    }

    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(_serviceType, _key, (sp, _) => Resolve(sp, _implementationType), lifetime);

        // yield return Factory(
        //     KeyValueType(_keyType, _serviceType),
        //     sp => KeyValue(_keyType, _serviceType, _key, Resolve(sp, _implementationType)),
        //     lifetime
        // );
    }
}
