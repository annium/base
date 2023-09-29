using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class TypeKeyedFactoryRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Type _implementationType;
    private readonly Type _keyType;
    private readonly object _key;

    public TypeKeyedFactoryRegistration(Type serviceType, Type implementationType, Type keyType, object key)
    {
        ServiceType = serviceType;
        _implementationType = implementationType;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, FactoryType(ServiceType)),
            sp => KeyValue(_keyType, FactoryType(ServiceType), _key, Expression.Lambda(Resolve(sp, _implementationType))),
            lifetime
        );
    }
}