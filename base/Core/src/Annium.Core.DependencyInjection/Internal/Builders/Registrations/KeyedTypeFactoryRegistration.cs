using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedTypeFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly Type _implementationType;
    private readonly Type _keyType;
    private readonly object _key;

    public KeyedTypeFactoryRegistration(Type serviceType, Type implementationType, Type keyType, object key)
    {
        _serviceType = serviceType;
        _implementationType = implementationType;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(_serviceType),
            sp => Expression.Lambda(Resolve(sp, _implementationType)),
            lifetime
        );

        // yield return Factory(
        //     KeyValueType(_keyType, FactoryType(_serviceType)),
        //     sp =>
        //         KeyValue(_keyType, FactoryType(_serviceType), _key, Expression.Lambda(Resolve(sp, _implementationType))),
        //     lifetime
        // );
    }
}
