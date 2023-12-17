using System;
using System.Collections.Generic;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class TypeRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly Type _implementationType;

    public TypeRegistration(Type serviceType, Type implementationType)
    {
        _serviceType = serviceType;
        _implementationType = implementationType;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        if (_implementationType == _serviceType || _implementationType.ContainsGenericParameters)
            yield return ServiceDescriptor.Type(_serviceType, _implementationType, lifetime);
        else
            yield return Factory(_serviceType, sp => Resolve(sp, _implementationType), lifetime);
    }
}
