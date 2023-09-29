using System;
using System.Collections.Generic;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class TypeRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Type _implementationType;

    public TypeRegistration(Type serviceType, Type implementationType)
    {
        ServiceType = serviceType;
        _implementationType = implementationType;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        if (_implementationType == ServiceType || _implementationType.ContainsGenericParameters)
            yield return ServiceDescriptor.Type(ServiceType, _implementationType, lifetime);
        else
            yield return Factory(ServiceType, sp => Resolve(sp, _implementationType), lifetime);
    }
}