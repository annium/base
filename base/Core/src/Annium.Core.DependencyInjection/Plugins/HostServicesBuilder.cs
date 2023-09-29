using System;
using Annium.Core.DependencyInjection.Internal.Packs;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// This is emulation class for compatibility with extensions, expecting HostBuilder pattern implementation
/// </summary>
/// <typeparam name="TServicePack"></typeparam>
public class HostServicesBuilder<TServicePack>
    where TServicePack : ServicePackBase, new()
{
    public HostServicesProvider Build()
    {
        ServiceProviderBuilder builder = new();
        builder.UseServicePack<TServicePack>();

        return new HostServicesProvider(builder.Build());
    }
}

public class HostServicesProvider
{
    public IServiceProvider Services { get; }

    public HostServicesProvider(IServiceProvider services)
    {
        Services = services;
    }
}