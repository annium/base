using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceDescriptorExtensions
{
    public static string ToReadableString(this IServiceDescriptor descriptor)
    {
        return descriptor switch
        {
            ITypeServiceDescriptor x => $"type {Name(x.ServiceType)} -> {Name(x.ImplementationType)}",
            IKeyedTypeServiceDescriptor x => $"type {Name(x.ServiceType)} [{x.Key}] -> {Name(x.ImplementationType)}",
            IFactoryServiceDescriptor x
                => $"factory {Name(x.ServiceType)} ->  {Name(x.ImplementationFactory.GetType())}",
            IKeyedFactoryServiceDescriptor x
                => $"factory {Name(x.ServiceType)} [{x.Key}] ->  {Name(x.ImplementationFactory.GetType())}",
            IInstanceServiceDescriptor x
                => $"instance {Name(x.ServiceType)} -> {Name(x.ImplementationInstance.GetType())}",
            IKeyedInstanceServiceDescriptor x
                => $"instance {Name(x.ServiceType)} [{x.Key}] -> {Name(x.ImplementationInstance.GetType())}",
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
        };

        static string Name(Type type) => $"{type.Namespace}.{type.FriendlyName()}";
    }

    public static MicrosoftServiceDescriptor ToMicrosoft(this IServiceDescriptor descriptor) =>
        descriptor switch
        {
            ITypeServiceDescriptor x
                => new MicrosoftServiceDescriptor(
                    x.ServiceType,
                    x.ImplementationType,
                    (MicrosoftServiceLifetime)x.Lifetime
                ),
            IKeyedTypeServiceDescriptor x
                => new MicrosoftServiceDescriptor(
                    x.ServiceType,
                    x.ImplementationType,
                    (MicrosoftServiceLifetime)x.Lifetime
                ),
            IFactoryServiceDescriptor x
                => new MicrosoftServiceDescriptor(
                    x.ServiceType,
                    x.ImplementationFactory,
                    (MicrosoftServiceLifetime)x.Lifetime
                ),
            IKeyedFactoryServiceDescriptor x
                => new MicrosoftServiceDescriptor(
                    x.ServiceType,
                    x.ImplementationFactory,
                    (MicrosoftServiceLifetime)x.Lifetime
                ),
            IInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationInstance),
            IKeyedInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationInstance),
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
        };
}
