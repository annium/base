using System;
using System.Collections.Generic;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class TypeKeyedRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Type _implementationType;
    private readonly Type _keyType;
    private readonly object _key;

    public TypeKeyedRegistration(Type serviceType, Type implementationType, Type keyType, object key)
    {
        ServiceType = serviceType;
        _implementationType = implementationType;
        _keyType = keyType;
        _key = key;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            KeyValueType(_keyType, ServiceType),
            sp => KeyValue(_keyType, ServiceType, _key, Resolve(sp, _implementationType)),
            lifetime
        );
    }
}