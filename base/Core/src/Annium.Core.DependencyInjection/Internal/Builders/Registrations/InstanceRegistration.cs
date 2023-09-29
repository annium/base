using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class InstanceRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly object _instance;

    public InstanceRegistration(Type serviceType, object instance)
    {
        ServiceType = serviceType;
        _instance = instance;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Instance(ServiceType, _instance, lifetime);
    }
}