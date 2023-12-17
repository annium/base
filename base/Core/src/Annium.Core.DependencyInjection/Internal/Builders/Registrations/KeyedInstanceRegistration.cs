using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedInstanceRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _key;
    private readonly object _instance;

    public KeyedInstanceRegistration(Type serviceType, object key, object instance)
    {
        _serviceType = serviceType;
        _key = key;
        _instance = instance;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(_serviceType, _key, (_, _) => Expression.Constant(_instance), lifetime);
    }
}
