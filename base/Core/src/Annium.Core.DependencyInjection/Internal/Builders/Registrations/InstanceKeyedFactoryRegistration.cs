using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class InstanceKeyedFactoryRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly object _instance;
    private readonly Type _keyType;
    private readonly object _key;

    public InstanceKeyedFactoryRegistration(Type serviceType, object instance, Type keyType, object key)
    {
        ServiceType = serviceType;
        _instance = instance;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, FactoryType(ServiceType)),
            _ => KeyValue(_keyType, FactoryType(ServiceType), _key, Expression.Lambda(Expression.Constant(_instance))),
            lifetime
        );
    }
}