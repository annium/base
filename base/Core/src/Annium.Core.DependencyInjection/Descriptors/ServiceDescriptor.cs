using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceDescriptor
{
    public static IServiceDescriptor From(MicrosoftServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationType is not null)
            return Type(descriptor.ServiceType, descriptor.ImplementationType, (ServiceLifetime)descriptor.Lifetime);

        if (descriptor.ImplementationFactory is not null)
            return Factory(descriptor.ServiceType, descriptor.ImplementationFactory, (ServiceLifetime)descriptor.Lifetime);

        if (descriptor.ImplementationInstance is not null)
            return Instance(descriptor.ServiceType, descriptor.ImplementationInstance, (ServiceLifetime)descriptor.Lifetime);

        throw new NotSupportedException($"{descriptor} has unsupported configuration");
    }

    public static ITypeServiceDescriptor Type<TService, TImplementation>(ServiceLifetime lifetime)
        where TImplementation : TService
        => new TypeServiceDescriptor
        {
            ServiceType = typeof(TService),
            ImplementationType = typeof(TImplementation),
            Lifetime = lifetime,
        };

    public static ITypeServiceDescriptor Type(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        => new TypeServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationType = implementationType,
            Lifetime = lifetime,
        };

    public static IFactoryServiceDescriptor Factory<T>(Func<IServiceProvider, T> implementationFactory, ServiceLifetime lifetime)
        where T : class
        => new FactoryServiceDescriptor
        {
            ServiceType = typeof(T),
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };

    public static IFactoryServiceDescriptor Factory(Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
        => new FactoryServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationFactory = implementationFactory,
            Lifetime = lifetime,
        };

    public static IInstanceServiceDescriptor Instance<T>(T implementationInstance, ServiceLifetime lifetime)
        where T : notnull
        => new InstanceServiceDescriptor
        {
            ServiceType = typeof(T),
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };

    public static IInstanceServiceDescriptor Instance(Type serviceType, object implementationInstance, ServiceLifetime lifetime)
        => new InstanceServiceDescriptor
        {
            ServiceType = serviceType,
            ImplementationInstance = implementationInstance,
            Lifetime = lifetime
        };
}

internal sealed record TypeServiceDescriptor : ITypeServiceDescriptor
{
    public ServiceLifetime Lifetime { get; init; }
    public Type ServiceType { get; init; } = default!;
    public Type ImplementationType { get; init; } = default!;
}

internal sealed record FactoryServiceDescriptor : IFactoryServiceDescriptor
{
    public ServiceLifetime Lifetime { get; init; }
    public Type ServiceType { get; init; } = default!;
    public Func<IServiceProvider, object> ImplementationFactory { get; init; } = default!;
}

internal sealed record InstanceServiceDescriptor : IInstanceServiceDescriptor
{
    public ServiceLifetime Lifetime { get; init; }
    public Type ServiceType { get; init; } = default!;
    public object ImplementationInstance { get; init; } = default!;
}