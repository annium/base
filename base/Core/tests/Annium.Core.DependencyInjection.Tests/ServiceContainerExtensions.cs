using System;
using System.Linq;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Testing;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Extension methods for testing service container functionality
/// </summary>
internal static class ServiceContainerExtensions
{
    /// <summary>
    /// Verifies that the container has the expected number of descriptors
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="count">The expected number of descriptors</param>
    public static void Has(this IServiceContainer container, int count)
    {
        container.Count.Is(count, $"Expected to have {count} descriptor registered, but found {container.Count}.");
    }

    /// <summary>
    /// Verifies that the container has the expected number of descriptors for a specific service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type to check</param>
    /// <param name="count">The expected number of descriptors</param>
    public static void Has(this IServiceContainer container, Type serviceType, int count)
    {
        var descriptors = container.Where(x => x.ServiceType == serviceType).ToArray();
        descriptors.Length.Is(
            count,
            $"Expected to have {count} {serviceType.FriendlyName()} descriptor registered, but found {descriptors.Length}."
        );
    }

    /// <summary>
    /// Verifies that the container has a singleton registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasSingleton(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that the container has a keyed singleton registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasSingleton(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Verifies that the container has a scoped registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasScoped(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Verifies that the container has a keyed scoped registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasScoped(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Verifies that the container has a transient registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasTransient(this IServiceContainer container, Type serviceType, Type implementationType)
    {
        container.Has(serviceType, implementationType, ServiceLifetime.Transient);
    }

    /// <summary>
    /// Verifies that the container has a keyed transient registration for the specified types
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    public static void HasTransient(
        this IServiceContainer container,
        Type serviceType,
        object key,
        Type implementationType
    )
    {
        container.Has(serviceType, key, implementationType, ServiceLifetime.Transient);
    }

    /// <summary>
    /// Verifies that the container has singleton type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasSingletonTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Singleton, count);
    }

    /// <summary>
    /// Verifies that the container has keyed singleton type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasSingletonTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Singleton, count);
    }

    /// <summary>
    /// Verifies that the container has scoped type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasScopedTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Scoped, count);
    }

    /// <summary>
    /// Verifies that the container has keyed scoped type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasScopedTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Scoped, count);
    }

    /// <summary>
    /// Verifies that the container has transient type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasTransientTypeFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasTypeFactory(serviceType, ServiceLifetime.Transient, count);
    }

    /// <summary>
    /// Verifies that the container has keyed transient type factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasTransientTypeFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasTypeFactory(serviceType, key, ServiceLifetime.Transient, count);
    }

    /// <summary>
    /// Verifies that the container has singleton Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasSingletonFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Singleton, count);
    }

    /// <summary>
    /// Verifies that the container has keyed singleton Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasSingletonFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Singleton, count);
    }

    /// <summary>
    /// Verifies that the container has scoped Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasScopedFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Scoped, count);
    }

    /// <summary>
    /// Verifies that the container has keyed scoped Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasScopedFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Scoped, count);
    }

    /// <summary>
    /// Verifies that the container has transient Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasTransientFuncFactory(this IServiceContainer container, Type serviceType, int count = 1)
    {
        container.HasFuncFactory(serviceType, ServiceLifetime.Transient, count);
    }

    /// <summary>
    /// Verifies that the container has keyed transient Func factory registrations for the specified service type
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="count">The expected number of factory registrations</param>
    public static void HasTransientFuncFactory(
        this IServiceContainer container,
        Type serviceType,
        object key,
        int count = 1
    )
    {
        container.HasFuncFactory(serviceType, key, ServiceLifetime.Transient, count);
    }

    /// <summary>
    /// Verifies that the container has a registration with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="implementationType">The implementation type</param>
    /// <param name="lifetime">The expected service lifetime</param>
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

    /// <summary>
    /// Verifies that the container has a keyed registration with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="implementationType">The implementation type</param>
    /// <param name="lifetime">The expected service lifetime</param>
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

    /// <summary>
    /// Verifies that the container has type factory registrations with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="lifetime">The expected service lifetime</param>
    /// <param name="count">The expected number of factory registrations</param>
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

    /// <summary>
    /// Verifies that the container has keyed type factory registrations with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="lifetime">The expected service lifetime</param>
    /// <param name="count">The expected number of factory registrations</param>
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

    /// <summary>
    /// Verifies that the container has Func factory registrations with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="lifetime">The expected service lifetime</param>
    /// <param name="count">The expected number of factory registrations</param>
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

    /// <summary>
    /// Verifies that the container has keyed Func factory registrations with the specified parameters
    /// </summary>
    /// <param name="container">The service container to verify</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="lifetime">The expected service lifetime</param>
    /// <param name="count">The expected number of factory registrations</param>
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

    /// <summary>
    /// Gets descriptors from the container for the specified service type and key
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <returns>Array of service descriptors matching the criteria</returns>
    private static IServiceDescriptor[] GetDescriptors(this IServiceContainer container, Type serviceType, object? key)
    {
        var descriptors = container.Where(x => x.ServiceType == serviceType && x.Key == key).ToArray();

        descriptors.Length.IsNotDefault($"No {serviceType.FriendlyName()} based descriptors found");

        return descriptors;
    }
}
