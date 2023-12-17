using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class InstanceFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _instance;

    public InstanceFactoryRegistration(Type serviceType, object instance)
    {
        _serviceType = serviceType;
        _instance = instance;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(_serviceType),
            _ => Expression.Lambda(Expression.Constant(_instance)),
            lifetime
        );
    }
}
