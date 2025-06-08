using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Packs;

/// <summary>
/// Internal implementation of service provider builder that manages service packs and builds service providers
/// </summary>
internal class ServiceProviderBuilder : IServiceProviderBuilder
{
    /// <summary>
    /// Flag indicating whether the service provider has already been built
    /// </summary>
    private bool _isAlreadyBuilt;

    /// <summary>
    /// The service container instance
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The collection of service packs to be configured and registered
    /// </summary>
    private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

    /// <summary>
    /// Initializes a new instance of the ServiceProviderBuilder class with an empty service container
    /// </summary>
    public ServiceProviderBuilder()
    {
        _container = new ServiceContainer();
    }

    /// <summary>
    /// Initializes a new instance of the ServiceProviderBuilder class with the specified service collection
    /// </summary>
    /// <param name="services">The service collection to initialize the container with</param>
    public ServiceProviderBuilder(IServiceCollection services)
    {
        _container = new ServiceContainer(services);
    }

    /// <summary>
    /// Adds a service pack of the specified type to the builder if not already added
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to add</typeparam>
    /// <returns>The current service provider builder instance</returns>
    public IServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        if (_packs.All(e => e.GetType() != typeof(TServicePack)))
            _packs.Add(new TServicePack());

        return this;
    }

    /// <summary>
    /// Adds the specified service pack instance to the builder
    /// </summary>
    /// <param name="servicePack">The service pack instance to add</param>
    /// <returns>The current service provider builder instance</returns>
    public IServiceProviderBuilder UseServicePack(ServicePackBase servicePack)
    {
        _packs.Add(servicePack);

        return this;
    }

    /// <summary>
    /// Builds the service provider by configuring, registering, and setting up all service packs
    /// </summary>
    /// <returns>The built service provider</returns>
    public ServiceProvider Build()
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("Entrypoint is already built");
        _isAlreadyBuilt = true;

        // configure all packs
        var configurationContainer = new ServiceContainer();
        foreach (var pack in _packs)
            pack.InternalConfigure(configurationContainer);

        // copy all configuration services to services
        foreach (var descriptor in configurationContainer)
            _container.Add(descriptor);

        // create provider from configurationServices
        var provider = _container.BuildServiceProvider();

        // register all services from packs
        foreach (var pack in _packs)
            pack.InternalRegister(_container, provider);

        // create provider from actual services
        provider = _container.BuildServiceProvider();

        // setup all services from packs
        foreach (var pack in _packs)
            pack.InternalSetup(provider);

        return provider;
    }
}
