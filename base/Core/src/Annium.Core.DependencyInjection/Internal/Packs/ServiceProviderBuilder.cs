using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Packs;

internal class ServiceProviderBuilder : IServiceProviderBuilder
{
    private bool _isAlreadyBuilt;

    private readonly IServiceContainer _container;

    private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

    public ServiceProviderBuilder()
    {
        _container = new ServiceContainer();
    }

    public ServiceProviderBuilder(IServiceCollection services)
    {
        _container = new ServiceContainer(services);
    }

    public IServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        if (_packs.All(e => e.GetType() != typeof(TServicePack)))
            _packs.Add(new TServicePack());

        return this;
    }

    public IServiceProviderBuilder UseServicePack(ServicePackBase servicePack)
    {
        _packs.Add(servicePack);

        return this;
    }

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