using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedInstanceFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _instance;
    private readonly Type _keyType;
    private readonly object _key;

    public KeyedInstanceFactoryRegistration(Type serviceType, object instance, Type keyType, object key)
    {
        _serviceType = serviceType;
        _instance = instance;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, FactoryType(_serviceType)),
            _ => KeyValue(_keyType, FactoryType(_serviceType), _key, Expression.Lambda(Expression.Constant(_instance))),
            lifetime
        );
    }
}
