using System;
using System.Collections.Generic;
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
        // Always emit a keyed factory descriptor that routes through the non-keyed implementation
        // registration. This is INTENTIONALLY asymmetric with TypeRegistration's same-types
        // optimization: keyed lookups must share the underlying singleton instance with non-keyed
        // lookups of the implementation type. A direct keyed-type descriptor would let M.E.DI
        // create a separate keyed singleton, breaking that shared-instance contract (covered by
        // SingleRegistrationTest.AsKeyedSelf_Works and BulkRegistrationTest.AsKeyedSelf_Works).
        yield return Factory(_serviceType, _key, (sp, _) => Resolve(sp, _implementationType), lifetime);
    }
}
