using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class FactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly Func<IServiceProvider, object> _factory;

    public FactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory)
    {
        _serviceType = serviceType;
        _factory = factory;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return ServiceDescriptor.Factory(_serviceType, _factory, lifetime);
    }
}
