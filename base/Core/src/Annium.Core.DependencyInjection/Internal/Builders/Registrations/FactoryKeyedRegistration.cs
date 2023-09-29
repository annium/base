using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class FactoryKeyedRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Func<IServiceProvider, object> _factory;
    private readonly Type _keyType;
    private readonly object _key;

    public FactoryKeyedRegistration(Type serviceType, Func<IServiceProvider, object> factory, Type keyType, object key)
    {
        ServiceType = serviceType;
        _factory = factory;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, ServiceType),
            sp => KeyValue(_keyType, ServiceType, _key, Expression.Invoke(Expression.Constant(_factory), sp)),
            lifetime
        );
    }
}