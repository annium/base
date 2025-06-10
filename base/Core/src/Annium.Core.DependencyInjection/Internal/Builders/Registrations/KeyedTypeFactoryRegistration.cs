using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.DependencyInjection.Descriptors;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for keyed type-based factories
/// </summary>
internal class KeyedTypeFactoryRegistration : IRegistration
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
    /// Initializes a new instance of the KeyedTypeFactoryRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    public KeyedTypeFactoryRegistration(Type serviceType, object key, Type implementationType)
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
        yield return Factory(
            FactoryType(_serviceType),
            _key,
            (sp, _) => Expression.Lambda(Resolve(sp, _implementationType)),
            lifetime
        );

        // yield return Factory(
        //     KeyValueType(_keyType, FactoryType(_serviceType)),
        //     sp =>
        //         KeyValue(_keyType, FactoryType(_serviceType), _key, Expression.Lambda(Resolve(sp, _implementationType))),
        //     lifetime
        // );
    }
}
