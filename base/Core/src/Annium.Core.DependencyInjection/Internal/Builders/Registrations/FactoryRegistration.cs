using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class FactoryRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Func<IServiceProvider, object> _factory;

    public FactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory)
    {
        ServiceType = serviceType;
        _factory = factory;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Factory(ServiceType, _factory, lifetime);
    }
}