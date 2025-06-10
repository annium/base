using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Packs;

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
    /// Builds the service provider with all configured service packs
    /// </summary>
    /// <returns>The built service provider</returns>
    ServiceProvider Build();
}
