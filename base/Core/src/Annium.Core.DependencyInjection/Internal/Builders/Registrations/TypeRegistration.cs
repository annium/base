using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Descriptors;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration for type-based services
/// </summary>
internal class TypeRegistration : IRegistration
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
    /// Initializes a new instance of the TypeRegistration class
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    public TypeRegistration(Type serviceType, Type implementationType)
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
        if (_implementationType == _serviceType || _implementationType.ContainsGenericParameters)
            yield return ServiceDescriptor.Type(_serviceType, _implementationType, lifetime);
        else
            yield return Factory(_serviceType, sp => Resolve(sp, _implementationType), lifetime);
    }
}
