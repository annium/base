using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class InstanceFactoryRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly object _instance;

    public InstanceFactoryRegistration(Type serviceType, object instance)
    {
        ServiceType = serviceType;
        _instance = instance;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(ServiceType),
            _ => Expression.Lambda(Expression.Constant(_instance)),
            lifetime
        );
    }
}