using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Extension methods for service descriptors
/// </summary>
public static class ServiceDescriptorExtensions
{
    /// <summary>
    /// Converts a service descriptor to a human-readable string representation
    /// </summary>
    /// <param name="descriptor">Service descriptor to convert</param>
    /// <returns>Human-readable string representation</returns>
    public static string ToReadableString(this IServiceDescriptor descriptor)
    {
        return descriptor switch
        {
            ITypeServiceDescriptor x => $"type {Name(x.ServiceType)} -> {Name(x.ImplementationType)}",
            IKeyedTypeServiceDescriptor x => $"type {Name(x.ServiceType)} [{x.Key}] -> {Name(x.ImplementationType)}",
            IFactoryServiceDescriptor x =>
                $"factory {Name(x.ServiceType)} ->  {Name(x.ImplementationFactory.GetType())}",
            IKeyedFactoryServiceDescriptor x =>
                $"factory {Name(x.ServiceType)} [{x.Key}] ->  {Name(x.ImplementationFactory.GetType())}",
            IInstanceServiceDescriptor x =>
                $"instance {Name(x.ServiceType)} -> {Name(x.ImplementationInstance.GetType())}",
            IKeyedInstanceServiceDescriptor x =>
                $"instance {Name(x.ServiceType)} [{x.Key}] -> {Name(x.ImplementationInstance.GetType())}",
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported"),
        };

        static string Name(Type type) => $"{type.Namespace}.{type.FriendlyName()}";
    }

    /// <summary>
    /// Converts a service descriptor to Microsoft service descriptor format
    /// </summary>
    /// <param name="descriptor">Service descriptor to convert</param>
    /// <returns>Microsoft service descriptor</returns>
    public static MicrosoftServiceDescriptor ToMicrosoft(this IServiceDescriptor descriptor) =>
        descriptor switch
        {
            ITypeServiceDescriptor x => new MicrosoftServiceDescriptor(
                x.ServiceType,
                x.ImplementationType,
                (MicrosoftServiceLifetime)x.Lifetime
            ),
            IKeyedTypeServiceDescriptor x => new MicrosoftServiceDescriptor(
                x.ServiceType,
                x.Key,
                x.ImplementationType,
                (MicrosoftServiceLifetime)x.Lifetime
            ),
            IFactoryServiceDescriptor x => new MicrosoftServiceDescriptor(
                x.ServiceType,
                x.ImplementationFactory,
                (MicrosoftServiceLifetime)x.Lifetime
            ),
            IKeyedFactoryServiceDescriptor x => new MicrosoftServiceDescriptor(
                x.ServiceType,
                x.Key,
                x.ImplementationFactory!,
                (MicrosoftServiceLifetime)x.Lifetime
            ),
            IInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationInstance),
            IKeyedInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(
                x.ServiceType,
                x.Key,
                x.ImplementationInstance
            ),
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported"),
        };
}
