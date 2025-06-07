using System;
using Annium.Core.DependencyInjection.Internal.Packs;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Factory for creating service provider builders and service providers
/// </summary>
public class ServiceProviderFactory : IServiceProviderFactory<IServiceProviderBuilder>
{
    /// <summary>
    /// The action to configure the service provider builder
    /// </summary>
    private readonly Action<ServiceProviderBuilder> _configure;

    /// <summary>
    /// Initializes a new instance of the ServiceProviderFactory class with no configuration
    /// </summary>
    public ServiceProviderFactory()
    {
        _configure = _ => { };
    }

    /// <summary>
    /// Initializes a new instance of the ServiceProviderFactory class with the specified configuration action
    /// </summary>
    /// <param name="configure">The action to configure the service provider builder</param>
    public ServiceProviderFactory(Action<IServiceProviderBuilder> configure)
    {
        _configure = configure;
    }

    /// <summary>
    /// Creates a service provider builder with the specified service collection
    /// </summary>
    /// <param name="services">The service collection to initialize the builder with</param>
    /// <returns>The created service provider builder</returns>
    public IServiceProviderBuilder CreateBuilder(IServiceCollection services)
    {
        var builder = new ServiceProviderBuilder(services);
        _configure(builder);

        return builder;
    }

    /// <summary>
    /// Creates a service provider from the specified builder
    /// </summary>
    /// <param name="container">The service provider builder to build from</param>
    /// <returns>The created service provider</returns>
    public IServiceProvider CreateServiceProvider(IServiceProviderBuilder container)
    {
        return ((ServiceProviderBuilder)container).Build();
    }
}
