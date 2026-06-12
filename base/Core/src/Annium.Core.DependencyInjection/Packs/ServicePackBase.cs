using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base class for service packs that provide modular service configuration
/// </summary>
public abstract class ServicePackBase
{
    /// <summary>
    /// Collection of nested service packs
    /// </summary>
    private readonly List<ServicePackBase> _packs = new();

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
    /// <param name="ct">Cancellation token observed cooperatively by async pack authors</param>
    /// <returns>A task that completes when configuration is done</returns>
    public virtual Task ConfigureAsync(IServiceContainer container, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Registers services in the container using the provided service provider for dependency resolution
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    /// <param name="ct">Cancellation token observed cooperatively by async pack authors</param>
    /// <returns>A task that completes when registration is done</returns>
    public virtual Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct) =>
        Task.CompletedTask;

    /// <summary>
    /// Performs final setup and initialization of services
    /// </summary>
    /// <param name="provider">The service provider for resolving services</param>
    /// <param name="ct">Cancellation token observed cooperatively by async pack authors</param>
    /// <returns>A task that completes when setup is done</returns>
    public virtual Task SetupAsync(IServiceProvider provider, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Internal walker — depth-first: children configure before parent, sequential within subtree
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="ct">Cancellation token threaded to each pack</param>
    /// <returns>A task that completes when the subtree is configured</returns>
    internal async Task InternalConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        foreach (var pack in _packs)
            await pack.InternalConfigureAsync(container, ct).ConfigureAwait(false);

        await ConfigureAsync(container, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Internal walker — depth-first: children register before parent, sequential within subtree
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    /// <param name="ct">Cancellation token threaded to each pack</param>
    /// <returns>A task that completes when the subtree is registered</returns>
    internal async Task InternalRegisterAsync(
        IServiceContainer container,
        IServiceProvider provider,
        CancellationToken ct
    )
    {
        foreach (var pack in _packs)
            await pack.InternalRegisterAsync(container, provider, ct).ConfigureAwait(false);

        await RegisterAsync(container, provider, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Internal walker — depth-first: children setup before parent, sequential within subtree
    /// </summary>
    /// <param name="provider">The service provider for resolving services</param>
    /// <param name="ct">Cancellation token threaded to each pack</param>
    /// <returns>A task that completes when the subtree is set up</returns>
    internal async Task InternalSetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        foreach (var pack in _packs)
            await pack.InternalSetupAsync(provider, ct).ConfigureAwait(false);

        await SetupAsync(provider, ct).ConfigureAwait(false);
    }
}
