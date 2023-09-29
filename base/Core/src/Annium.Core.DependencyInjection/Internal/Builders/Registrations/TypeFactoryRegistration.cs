using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

internal class TypeFactoryRegistration : IRegistration
{
    public Type ServiceType { get; }
    private readonly Type _implementationType;

    public TypeFactoryRegistration(Type serviceType, Type implementationType)
    {
        ServiceType = serviceType;
        _implementationType = implementationType;
    }

    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(ServiceType),
            sp => Expression.Lambda(Resolve(sp, _implementationType)),
            lifetime
        );
    }
}