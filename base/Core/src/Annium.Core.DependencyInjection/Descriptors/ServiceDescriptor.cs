using System;
using Annium.Core.DependencyInjection.Internal.Descriptors;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceDescriptor
{
    public static IServiceDescriptor From(MicrosoftServiceDescriptor descriptor)
    {
        if (descriptor.ServiceKey is not null)
        {
            if (descriptor.ImplementationType is not null)
                return Type(
                    descriptor.ServiceType,
                    descriptor.ServiceKey,
                    descriptor.ImplementationType,
                    (ServiceLifetime)descriptor.Lifetime
                );

            if (descriptor.ImplementationFactory is not null)
                return Factory(
                    descriptor.ServiceType,
                    descriptor.ServiceKey,
                    descriptor.ImplementationFactory,
                    (ServiceLifetime)descriptor.Lifetime
                );

            if (descriptor.ImplementationInstance is not null)
                return Instance(
                    descriptor.ServiceType,
                    descriptor.ServiceKey,
                    descriptor.ImplementationInstance,
                    (ServiceLifetime)descriptor.Lifetime
                );
        }

        if (descriptor.ImplementationType is not null)
            return Type(descriptor.ServiceType, descriptor.ImplementationType, (ServiceLifetime)descriptor.Lifetime);

        if (descriptor.ImplementationFactory is not null)
            return Factory(
                descriptor.ServiceType,
                descriptor.ImplementationFactory,
                (ServiceLifetime)descriptor.Lifetime
            );

        if (descriptor.ImplementationInstance is not null)
            return Instance(
                descriptor.ServiceType,
                descriptor.ImplementationInstance,
                (ServiceLifetime)descriptor.Lifetime
            );

        throw new NotSupportedException($"{descriptor} has unsupported configuration");
    }

    public static ITypeServiceDescriptor Type<TService, TImplementation>(ServiceLifetime lifetime)
        where TImplementation : TService
    {
        return new TypeServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImplementation),
            Lifetime = lifetime,
        };
    }

    public static IKeyedTypeServiceDescriptor Type<TService, TImplementation>(object key, ServiceLifetime lifetime)
        where TImplementation : TService
    {
        return new KeyedTypeServiceDescriptor
        {
            ServiceType = typeof(TService),
            Key = key,
            ImplementationType = typeof(TImplementation),
            Lifetime = lifetime,
        };
    }

    public static ITypeServiceDescriptor Type(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        return new TypeServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
        };
    }

    public static IKeyedTypeServiceDescriptor Type(
        Type serviceType,
        object key,
        Type implementationType,
        ServiceLifetime lifetime
    )
    {
        return new KeyedTypeServiceDescriptor
        {
            ServiceType = serviceType,
            Key = key,
            ImplementationType = implementationType,
            Lifetime = lifetime,
        };
    }

    public static IFactoryServiceDescriptor Factory<T>(
        Func<IServiceProvider, T> implementationFactory,
        ServiceLifetime lifetime
    )
        where T : class
    {
        return new FactoryServiceDescriptor
        {
            ServiceType = typeof(T),
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };
    }

    public static IKeyedFactoryServiceDescriptor Factory<T>(
        object key,
        Func<IServiceProvider, T> implementationFactory,
        ServiceLifetime lifetime
    )
        where T : class
    {
        return new KeyedFactoryServiceDescriptor
        {
            ServiceType = typeof(T),
            Key = key,
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };
    }

    public static IFactoryServiceDescriptor Factory(
        Type serviceType,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime
    )
    {
        return new FactoryServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };
    }

    public static IKeyedFactoryServiceDescriptor Factory(
        Type serviceType,
        object key,
        Func<IServiceProvider, object> implementationFactory,
        ServiceLifetime lifetime
    )
    {
        return new KeyedFactoryServiceDescriptor
        {
            ServiceType = serviceType,
            Key = key,
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };
    }

    public static IInstanceServiceDescriptor Instance<T>(T implementationInstance, ServiceLifetime lifetime)
        where T : notnull
    {
        return new InstanceServiceDescriptor
        {
            ServiceType = typeof(T),
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };
    }

    public static IKeyedInstanceServiceDescriptor Instance<T>(
        object key,
        T implementationInstance,
        ServiceLifetime lifetime
    )
        where T : notnull
    {
        return new KeyedInstanceServiceDescriptor
        {
            ServiceType = typeof(T),
            Key = key,
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };
    }

    public static IInstanceServiceDescriptor Instance(
        Type serviceType,
        object implementationInstance,
        ServiceLifetime lifetime
    )
    {
        return new InstanceServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };
    }

    public static IKeyedInstanceServiceDescriptor Instance(
        Type serviceType,
        object key,
        object implementationInstance,
        ServiceLifetime lifetime
    )
    {
        return new KeyedInstanceServiceDescriptor
        {
            ServiceType = serviceType,
            Key = key,
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };
    }
}
