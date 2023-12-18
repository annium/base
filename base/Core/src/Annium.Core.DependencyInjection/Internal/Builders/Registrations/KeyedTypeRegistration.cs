using System;
using System.Collections.Generic;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class KeyedTypeRegistration : IRegistration
{
    private readonly Type _serviceType;
    private readonly object _key;
    private readonly Type _implementationType;

    public KeyedTypeRegistration(Type serviceType, object key, Type implementationType)
    {
        _serviceType = serviceType;
        _key = key;
        _implementationType = implementationType;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(_serviceType, _key, (sp, _) => Resolve(sp, _implementationType), lifetime);

        // yield return Factory(
        //     KeyValueType(_keyType, _serviceType),
        //     sp => KeyValue(_keyType, _serviceType, _key, Resolve(sp, _implementationType)),
        //     lifetime
        // );
    }
}
