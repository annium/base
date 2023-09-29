using System;
using Annium.Core.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter;

internal static class AdapterServiceProviderBuilder
{
    public static IServiceProvider Build(IDiscoveryContext discoveryContext)
    {
        var container = new ServiceContainer();
        container.Add(TestingConfigurationReader.Read(discoveryContext)).AsSelf().Singleton();

        var factory = new ServiceProviderFactory();

        return factory.CreateServiceProvider(factory.CreateBuilder(container.Collection).UseServicePack<Testing.ServicePack>());
    }
}