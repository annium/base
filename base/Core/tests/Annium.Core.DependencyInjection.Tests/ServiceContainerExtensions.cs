using System;
using System.Linq;
using Annium.Testing;

namespace Annium.Core.DependencyInjection.Tests;

internal static class ServiceContainerExtensions
{
    public static void Has(this IServiceContainer container, int count)
    {
        container.Count.Is(count, $"Expected to have {count} descriptor registered, but found {container.Count}.");
    }

    public static void Has(this IServiceContainer container, Type serviceType, int count)
    {
        var descriptors = container.Where(x => x.ServiceType == serviceType).ToArray();
        descriptors.Length.Is(
            count,
            $"Expected to have {count} {serviceType.FriendlyName()} descriptor registered, but found {descriptors.Length}."
        );
    }

    public static void HasSingleton(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Singleton);
    }

    public static void HasSingleton(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Singleton);
    }

    public static void HasScoped(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Scoped);
    }

    public static void HasScoped(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Scoped);
    }

    public static void HasTransient(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Transient);
    }

    public static void HasTransient(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Transient);
    }

    public static void HasSingletonTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Singleton, count);
    }

    public static void HasSingletonTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Singleton, count);
    }

    public static void HasScopedTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Scoped, count);
    }

    public static void HasScopedTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Scoped, count);
    }

    public static void HasTransientTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Transient, count);
    }

    public static void HasTransientTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Transient, count);
    }

    public static void HasSingletonFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Singleton, count);
    }

    public static void HasSingletonFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Singleton, count);
    }

    public static void HasScopedFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Scoped, count);
    }

    public static void HasScopedFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Scoped, count);
    }

    public static void HasTransientFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Transient, count);
    }

    public static void HasTransientFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Transient, count);
    }

    private static void Has(
        this IServiceContainer container,
        Type serviceType,
        Type implementationType,
        ServiceLifetime lifetime
    )
    {
        var descriptor = container
            .GetDescriptors(serviceType, null)
            .OfType<ITypeServiceDescriptor>()
            .SingleOrDefault(x => x.ImplementationType == implementationType);
        descriptor.IsNotDefault(
            $"Not found {serviceType.FriendlyName()} -> {implementationType.FriendlyName()} descriptor"
        );
        descriptor.Lifetime.Is(
            lifetime,
            $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
        );
    }

    private static void Has(
        this IServiceContainer container,
        Type serviceType,
        object? key,
        Type implementationType,
        ServiceLifetime lifetime
    )
    {
        var descriptor = container
            .GetDescriptors(serviceType, key)
            .OfType<IKeyedTypeServiceDescriptor>()
            .SingleOrDefault(x => x.ImplementationType == implementationType);
        descriptor.IsNotDefault(
            $"Not found {serviceType.FriendlyName()} -> {implementationType.FriendlyName()} descriptor"
        );
        descriptor.Lifetime.Is(
            lifetime,
            $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
        );
    }

    private static void HasTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        ServiceLifetime lifetime,
        int count
    )
    {
        var descriptors = container.GetDescriptors(serviceType, null).OfType<IFactoryServiceDescriptor>().ToArray();
        descriptors.Length.Is(
            count,
            $"Expected {count} {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}"
        );
        foreach (var descriptor in descriptors)
            descriptor.Lifetime.Is(
                lifetime,
                $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
            );
    }

    private static void HasTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object? key,
        ServiceLifetime lifetime,
        int count
    )
    {
        var descriptors = container.GetDescriptors(serviceType, key).OfType<IKeyedFactoryServiceDescriptor>().ToArray();
        descriptors.Length.Is(
            count,
            $"Expected {count} {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}"
        );
        foreach (var descriptor in descriptors)
            descriptor.Lifetime.Is(
                lifetime,
                $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
            );
    }

    private static void HasFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        ServiceLifetime lifetime,
        int count
    )
    {
        var descriptors = container
            .GetDescriptors(typeof(Func<>).MakeGenericType(serviceType), null)
            .OfType<IFactoryServiceDescriptor>()
            .ToArray();
        descriptors.Length.Is(
            count,
            $"Expected {count} () => {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}"
        );
        foreach (var descriptor in descriptors)
            descriptor.Lifetime.Is(
                lifetime,
                $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
            );
    }

    private static void HasFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object? key,
        ServiceLifetime lifetime,
        int count
    )
    {
        var descriptors = container
            .GetDescriptors(typeof(Func<>).MakeGenericType(serviceType), key)
            .OfType<IKeyedFactoryServiceDescriptor>()
            .ToArray();
        descriptors.Length.Is(
            count,
            $"Expected {count} () => {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}"
        );
        foreach (var descriptor in descriptors)
            descriptor.Lifetime.Is(
                lifetime,
                $"Descriptor {descriptor.ToReadableString()} is {descriptor.Lifetime}, but {lifetime} expected."
            );
    }

    private static IServiceDescriptor[] GetDescriptors(this IServiceContainer container, Type serviceType, object? key)
    {
        var descriptors = container.Where(x => x.ServiceType == serviceType && x.Key == key).ToArray();

        descriptors.Length.IsNotDefault($"No {serviceType.FriendlyName()} based descriptors found");

        return descriptors;
    }
}
