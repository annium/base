using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.DependencyInjection.Descriptors;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for keyed factory-based services
/// </summary>
internal class KeyedFactoryRegistration : IRegistration
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
    /// The keyed factory function to create service instances
    /// </summary>
    private readonly Func<IServiceProvider, object, object> _factory;

    /// <summary>
    /// Initializes a new instance of the KeyedFactoryRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="factory">The keyed factory function</param>
    public KeyedFactoryRegistration(Type serviceType, object key, Func<IServiceProvider, object, object> factory)
    {
        _serviceType = serviceType;
        _key = key;
        _factory = factory;
    }

    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            _serviceType,
            _key,
            (sp, key) => Expression.Invoke(Expression.Constant(_factory), sp, key),
            lifetime
        );

        // yield return Factory(
        //     KeyValueType(_keyType, _serviceType),
        //     sp => KeyValue(_keyType, _serviceType, _key, Expression.Invoke(Expression.Constant(_factory), sp)),
        //     lifetime
        // );
    }
}
