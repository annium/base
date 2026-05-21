using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Defines the contract for building service providers with service packs
/// </summary>
public interface IServiceProviderBuilder
{
    /// <summary>
    /// Adds a service pack of the specified type to the builder
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to add</typeparam>
    /// <returns>The current service provider builder instance</returns>
    IServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new();

    /// <summary>
    /// Adds the specified service pack instance to the builder
    /// </summary>
    /// <param name="servicePack">The service pack instance to add</param>
    /// <returns>The current service provider builder instance</returns>
    IServiceProviderBuilder UseServicePack(ServicePackBase servicePack);

    /// <summary>
    /// Asynchronously builds the service provider with all configured service packs.
    /// Honours cooperative cancellation at every pack await and at Phase 3→4 / Phase 4→5 boundaries.
    /// On any non-normal exit, partial state is disposed in reverse order (final before transient).
    /// </summary>
    /// <param name="ct">Cancellation token threaded to every pack phase</param>
    /// <returns>The built service provider</returns>
    Task<ServiceProvider> BuildAsync(CancellationToken ct);
}
