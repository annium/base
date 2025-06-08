using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for instance-based factories
/// </summary>
internal class InstanceFactoryRegistration : IRegistration
{
    /// <summary>
    /// The service type for this registration
    /// </summary>
    private readonly Type _serviceType;

    /// <summary>
    /// The instance to wrap in a factory
    /// </summary>
    private readonly object _instance;

    /// <summary>
    /// Initializes a new instance of the InstanceFactoryRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="instance">The instance to wrap in a factory</param>
    public InstanceFactoryRegistration(Type serviceType, object instance)
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
        yield return Factory(
            FactoryType(_serviceType),
            _ => Expression.Lambda(Expression.Constant(_instance)),
            lifetime
        );
    }
}
