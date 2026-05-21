using System;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly Action<IServiceProviderBuilder> _configure;

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
    /// Annium-native async path. Use this from any caller that has a CancellationToken
    /// (Entrypoint, TestBase, ad-hoc bootstrap).
    /// </summary>
    /// <param name="container">The service provider builder to build from</param>
    /// <param name="ct">Cancellation token threaded to every pack phase</param>
    /// <returns>The built service provider</returns>
    public Task<ServiceProvider> CreateServiceProviderAsync(IServiceProviderBuilder container, CancellationToken ct) =>
        container.BuildAsync(ct);

    /// <summary>
    /// M.E.DI host bridge — the one documented sync-over-async in the framework, required by
    /// <see cref="IServiceProviderFactory{TContainer}"/>'s sync contract. ASP.NET Core's
    /// <c>UseServiceProviderFactory</c> and Blazor's <c>ConfigureContainer</c> invoke this
    /// synchronously. All Annium-native callers go through <see cref="CreateServiceProviderAsync"/>.
    /// </summary>
    /// <param name="container">The service provider builder to build from</param>
    /// <returns>The created service provider</returns>
#pragma warning disable VSTHRD002
    public IServiceProvider CreateServiceProvider(IServiceProviderBuilder container) =>
        container.BuildAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
}
