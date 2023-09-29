using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Resources;
using Annium.Core.Runtime.Internal.Time;
using Annium.Core.Runtime.Internal.Types;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Time;
using Annium.Core.Runtime.Types;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRuntime(
        this IServiceContainer container,
        Assembly assembly
    )
    {
        container.Add(TypeManager.GetInstance(assembly, VoidLogger.Instance)).As<ITypeManager>().Singleton();
        container.Add<ITypeResolver, TypeResolver>().Singleton();

        return container;
    }

    public static IServiceContainer AddResourceLoader(this IServiceContainer container)
    {
        container.Add<IResourceLoader, ResourceLoader>().Singleton();

        return container;
    }

    public static ITimeConfigurationBuilder AddTime(this IServiceContainer container, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        => new TimeConfigurationBuilder(container, serviceLifetime);

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

    public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container)
        => container.Add(container.GetTypeManager().Types.AsEnumerable());

    public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly)
        => container.Add(assembly.GetTypes().AsEnumerable());
}