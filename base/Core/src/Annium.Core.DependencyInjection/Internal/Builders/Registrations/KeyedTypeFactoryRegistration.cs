using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedTypeFactoryRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _key;
    private readonly Type _implementationType;

    public KeyedTypeFactoryRegistration(Type serviceType, object key, Type implementationType)
    {
        _serviceType = serviceType;
        _key = key;
        _implementationType = implementationType;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(_serviceType),
            _key,
            (sp, key) => Expression.Lambda(Resolve(sp, key, _implementationType)),
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
