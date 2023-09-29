using System;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceDescriptorExtensions
{
    public static string ToReadableString(this IServiceDescriptor descriptor) => descriptor switch
    {
        ITypeServiceDescriptor x     => $"type {x.ServiceType.Namespace}.{x.ServiceType.FriendlyName()} -> {x.ImplementationType.Namespace}.{x.ImplementationType.FriendlyName()}",
        IFactoryServiceDescriptor x  => $"factory {x.ServiceType.Namespace}.{x.ServiceType.FriendlyName()} ->  {x.ImplementationFactory.GetType().Namespace}.{x.ImplementationFactory.GetType().FriendlyName()}",
        IInstanceServiceDescriptor x => $"instance {x.ServiceType.Namespace}.{x.ServiceType.FriendlyName()} -> {x.ImplementationInstance.GetType().Namespace}.{x.ImplementationInstance.GetType().FriendlyName()}",
        _                            => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
    };

    public static MicrosoftServiceDescriptor ToMicrosoft(this IServiceDescriptor descriptor) => descriptor switch
    {
        ITypeServiceDescriptor x     => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationType, (MicrosoftServiceLifetime)x.Lifetime),
        IFactoryServiceDescriptor x  => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationFactory, (MicrosoftServiceLifetime)x.Lifetime),
        IInstanceServiceDescriptor x => new MicrosoftServiceDescriptor(x.ServiceType, x.ImplementationInstance),
        _                            => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
    };
}