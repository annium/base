using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.DependencyInjection.Descriptors;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for type-based factories
/// </summary>
internal class TypeFactoryRegistration : IRegistration
{
    /// <summary>
    /// The service type for this registration
    /// </summary>
    private readonly Type _serviceType;

    /// <summary>
    /// The implementation type for this service
    /// </summary>
    private readonly Type _implementationType;

    /// <summary>
    /// Initializes a new instance of the TypeFactoryRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    public TypeFactoryRegistration(Type serviceType, Type implementationType)
    {
        _serviceType = serviceType;
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
            sp => Expression.Lambda(Resolve(sp, _implementationType)),
            lifetime
        );
    }
}
