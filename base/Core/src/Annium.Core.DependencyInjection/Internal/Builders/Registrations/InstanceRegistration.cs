using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class InstanceRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _instance;

    public InstanceRegistration(Type serviceType, object instance)
    {
        _serviceType = serviceType;
        _instance = instance;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Instance(_serviceType, _instance, lifetime);
    }
}
