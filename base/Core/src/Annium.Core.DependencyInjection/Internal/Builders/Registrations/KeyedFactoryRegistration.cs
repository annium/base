using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly Func<IServiceProvider, object> _factory;
    private readonly Type _keyType;
    private readonly object _key;

    public KeyedFactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory, Type keyType, object key)
    {
        _serviceType = serviceType;
        _factory = factory;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, _serviceType),
            sp => KeyValue(_keyType, _serviceType, _key, Expression.Invoke(Expression.Constant(_factory), sp)),
            lifetime
        );
    }
}
