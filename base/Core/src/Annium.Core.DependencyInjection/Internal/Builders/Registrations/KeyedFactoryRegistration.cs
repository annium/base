using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _key;
    private readonly Func<IServiceProvider, object, object> _factory;

    public KeyedFactoryRegistration(Type serviceType, object key, Func<IServiceProvider, object, object> factory)
    {
        _serviceType = serviceType;
        _key = key;
        _factory = factory;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            _serviceType,
            _key,
            (sp, key) => Expression.Invoke(Expression.Constant(_factory), sp, key),
            lifetime
        );

        // yield return Factory(
        //     KeyValueType(_keyType, _serviceType),
        //     sp => KeyValue(_keyType, _serviceType, _key, Expression.Invoke(Expression.Constant(_factory), sp)),
        //     lifetime
        // );
    }
}
