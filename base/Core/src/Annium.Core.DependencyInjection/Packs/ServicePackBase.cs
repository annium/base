using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Core.DependencyInjection.Packs;

/// <summary>
/// Base class for service packs that provide modular service configuration
/// </summary>
public abstract class ServicePackBase
{
    /// <summary>
    /// Collection of nested service packs
    /// </summary>
    private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

    /// <summary>
    /// Adds a nested service pack of the specified type
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to add</typeparam>
    public void Add<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        _packs.Add(new TServicePack());
    }

    /// <summary>
    /// Configures the service container with services needed for dependency resolution
    /// </summary>
    /// <param name="container">The service container to configure</param>
    public virtual void Configure(IServiceContainer container) { }

    /// <summary>
    /// Registers services in the container using the provided service provider for dependency resolution
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    public virtual void Register(IServiceContainer container, IServiceProvider provider) { }

    /// <summary>
    /// Performs final setup and initialization of services
    /// </summary>
    /// <param name="provider">The service provider for resolving services</param>
    public virtual void Setup(IServiceProvider provider) { }

    /// <summary>
    /// Internal method that configures nested packs and then this pack
    /// </summary>
    /// <param name="container">The service container to configure</param>
    internal void InternalConfigure(IServiceContainer container)
    {
        foreach (var pack in _packs)
            pack.InternalConfigure(container);

        Configure(container);
    }

    /// <summary>
    /// Internal method that registers services for nested packs and then this pack
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    internal void InternalRegister(IServiceContainer container, IServiceProvider provider)
    {
        foreach (var pack in _packs)
            pack.InternalRegister(container, provider);

        Register(container, provider);
    }

    /// <summary>
    /// Internal method that sets up nested packs and then this pack
    /// </summary>
    /// <param name="provider">The service provider for resolving services</param>
    internal void InternalSetup(IServiceProvider provider)
    {
        foreach (var pack in _packs)
            pack.InternalSetup(provider);

        Setup(provider);
    }
}
