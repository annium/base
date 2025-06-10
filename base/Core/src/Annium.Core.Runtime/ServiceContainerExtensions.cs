using System;
using System.Linq;
using System.Reflection;
using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime.Internal.Resources;
using Annium.Core.Runtime.Internal.Time;
using Annium.Core.Runtime.Internal.Types;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Time;
using Annium.Core.Runtime.Types;
using Annium.Logging;

namespace Annium.Core.Runtime;

/// <summary>
/// Provides extension methods for configuring runtime services in the dependency injection container.
/// </summary>
/// <remarks>
/// This class contains methods for adding core runtime services, resource loading, and time management
/// to the service container.
/// </remarks>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds core runtime services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="assembly">The assembly to scan for types.</param>
    /// <returns>The configured service container.</returns>
    /// <remarks>
    /// This method registers the type manager and type resolver as singleton services.
    /// Example usage:
    /// <code>
    /// var container = new ServiceContainer();
    /// container.AddRuntime(typeof(Program).Assembly);
    /// </code>
    /// </remarks>
    public static IServiceContainer AddRuntime(this IServiceContainer container, Assembly assembly)
    {
        container.Add(TypeManager.GetInstance(assembly, VoidLogger.Instance)).As<ITypeManager>().Singleton();
        container.Add<ITypeResolver, TypeResolver>().Singleton();

        return container;
    }

    /// <summary>
    /// Adds resource loading services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    /// <remarks>
    /// Registers the resource loader as a singleton service.
    /// Example usage:
    /// <code>
    /// var container = new ServiceContainer();
    /// container.AddResourceLoader();
    /// </code>
    /// </remarks>
    public static IServiceContainer AddResourceLoader(this IServiceContainer container)
    {
        container.Add<IResourceLoader, ResourceLoader>().Singleton();

        return container;
    }

    /// <summary>
    /// Adds time management services to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="serviceLifetime">The lifetime of the time services. Defaults to Singleton.</param>
    /// <returns>A time configuration builder for further configuration.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var container = new ServiceContainer();
    /// container.AddTime()
    ///     .UseSystemTime() // or other time provider configuration
    ///     .Build();
    /// </code>
    /// </remarks>
    public static ITimeConfigurationBuilder AddTime(
        this IServiceContainer container,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
    ) => new TimeConfigurationBuilder(container, serviceLifetime);

    /// <summary>
    /// Gets the registered type manager instance from the service container.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <returns>The registered type manager instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no type manager is registered or when multiple instances are found.</exception>
    /// <remarks>
    /// This method ensures that exactly one type manager instance is registered in the container.
    /// </remarks>
    public static ITypeManager GetTypeManager(this IServiceContainer container)
    {
        var descriptors = container.Where(x => x.ServiceType == typeof(ITypeManager)).ToArray();

        if (descriptors.Length != 1)
            throw new InvalidOperationException($"Single {nameof(ITypeManager)} instance must be registered.");

        var descriptor = descriptors[0];
        if (descriptor is IInstanceServiceDescriptor instanceDescriptor)
            return (ITypeManager)instanceDescriptor.ImplementationInstance;

        throw new InvalidOperationException($"{nameof(ITypeManager)} must be registered with instance.");
    }

    /// <summary>
    /// Adds all types from the type manager to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>A bulk registration builder for further configuration.</returns>
    /// <remarks>
    /// This method uses the registered type manager to get all available types for registration.
    /// </remarks>
    public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container) =>
        container.Add(container.GetTypeManager().Types.AsEnumerable());

    /// <summary>
    /// Adds all types from the specified assembly to the service container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="assembly">The assembly to scan for types.</param>
    /// <returns>A bulk registration builder for further configuration.</returns>
    /// <remarks>
    /// This method directly scans the provided assembly for types to register.
    /// </remarks>
    public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly) =>
        container.Add(assembly.GetTypes().AsEnumerable());
}
