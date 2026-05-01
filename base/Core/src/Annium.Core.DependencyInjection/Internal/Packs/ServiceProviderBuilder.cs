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
    /// Builds the service provider by configuring, registering, and setting up all service packs.
    /// <para>
    /// The three-phase <see cref="ServicePackBase"/> model is preserved: <c>Configure</c> populates
    /// a staging container, a transient provider is materialized for <c>Register</c> (so packs can
    /// consume Configure-phase services), then a final provider is built for <c>Setup</c>. The
    /// transient provider is disposed before <c>Setup</c> runs to release any singletons it
    /// materialized.
    /// </para>
    /// <para>
    /// The builder is single-use: a second call throws <see cref="InvalidOperationException"/>.
    /// A throw during Configure or Register leaves the builder in its pre-build state (the
    /// "already built" flag is only set after a successful build), so the caller can retry after
    /// addressing the fault or diagnose with fresh context.
    /// </para>
    /// </summary>
    /// <returns>The built service provider</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder has already produced a provider successfully.</exception>
    public ServiceProvider Build()
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("ServiceProviderBuilder is already built");

        ServiceProvider? transientProvider = null;
        ServiceProvider finalProvider;
        try
        {
            // Phase 1: Configure — accumulate in a staging container so a throw leaves _container intact
            var configurationContainer = new ServiceContainer();
            foreach (var pack in _packs)
                pack.InternalConfigure(configurationContainer);

            // merge staging → main container
            foreach (var descriptor in configurationContainer)
                _container.Add(descriptor);

            // Phase 2: build transient provider for Register's provider parameter
            transientProvider = _container.BuildServiceProvider();

            // Phase 3: Register — packs may consume Configure-phase services via transientProvider
            // while adding additional registrations to _container
            foreach (var pack in _packs)
                pack.InternalRegister(_container, transientProvider);

            // Phase 4: build the final provider capturing both Configure and Register registrations
            finalProvider = _container.BuildServiceProvider();
        }
        catch
        {
            // leave _isAlreadyBuilt false so the caller can retry after addressing the fault
            transientProvider?.Dispose();
            throw;
        }

        // transient is no longer needed — dispose it before Setup runs to release any singletons
        // it materialized during Register
        transientProvider.Dispose();

        _isAlreadyBuilt = true;

        // Phase 5: Setup on the final provider. If a pack throws here, finalProvider
        // is already constructed and would otherwise leak (it isn't returned to the
        // caller). Dispose it so the caller doesn't have to chase a hidden leak after
        // a Setup failure.
        try
        {
            foreach (var pack in _packs)
                pack.InternalSetup(finalProvider);
        }
        catch
        {
            finalProvider.Dispose();
            throw;
        }

        return finalProvider;
    }
}
