using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// A dynamic service pack that allows configuring services through delegate actions
/// </summary>
public class DynamicServicePack : ServicePackBase
{
    /// <summary>
    /// The delegate to configure services in the container
    /// </summary>
    private Func<IServiceContainer, CancellationToken, Task> _configure = (_, _) => Task.CompletedTask;

    /// <summary>
    /// The delegate to register services in the container
    /// </summary>
    private Func<IServiceContainer, IServiceProvider, CancellationToken, Task> _register = (_, _, _) =>
        Task.CompletedTask;

    /// <summary>
    /// The delegate to setup services using the provider
    /// </summary>
    private Func<IServiceProvider, CancellationToken, Task> _setup = (_, _) => Task.CompletedTask;

    /// <summary>
    /// Sets the configuration delegate for this service pack
    /// </summary>
    /// <param name="configure">The async delegate to configure services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Configure(Func<IServiceContainer, CancellationToken, Task> configure)
    {
        _configure = configure;
        return this;
    }

    /// <summary>
    /// Sets the configuration action for this service pack — ergonomic sync forwarder
    /// </summary>
    /// <param name="configure">The action to configure services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Configure(Action<IServiceContainer> configure)
    {
        _configure = (c, _) =>
        {
            configure(c);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Sets the registration delegate for this service pack
    /// </summary>
    /// <param name="register">The async delegate to register services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Register(Func<IServiceContainer, IServiceProvider, CancellationToken, Task> register)
    {
        _register = register;
        return this;
    }

    /// <summary>
    /// Sets the registration action for this service pack — ergonomic sync forwarder
    /// </summary>
    /// <param name="register">The action to register services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Register(Action<IServiceContainer, IServiceProvider> register)
    {
        _register = (c, p, _) =>
        {
            register(c, p);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Sets the setup delegate for this service pack
    /// </summary>
    /// <param name="setup">The async delegate to setup services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Setup(Func<IServiceProvider, CancellationToken, Task> setup)
    {
        _setup = setup;
        return this;
    }

    /// <summary>
    /// Sets the setup action for this service pack — ergonomic sync forwarder
    /// </summary>
    /// <param name="setup">The action to setup services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Setup(Action<IServiceProvider> setup)
    {
        _setup = (p, _) =>
        {
            setup(p);
            return Task.CompletedTask;
        };
        return this;
    }

    /// <summary>
    /// Runs the configure-phase delegate supplied to this pack, registering configuration-time services into the container.
    /// </summary>
    /// <param name="container">The service container being configured.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    /// <returns>A task that completes when the configure delegate finishes.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct) => _configure(container, ct);

    /// <summary>
    /// Runs the register-phase delegate supplied to this pack, registering services that may depend on the configure-phase provider.
    /// </summary>
    /// <param name="container">The service container to register services into.</param>
    /// <param name="provider">Provider exposing services registered during the configure phase.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    /// <returns>A task that completes when the register delegate finishes.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct) =>
        _register(container, provider, ct);

    /// <summary>
    /// Runs the setup-phase delegate supplied to this pack, performing post-registration initialization against the built provider.
    /// </summary>
    /// <param name="provider">The fully built service provider.</param>
    /// <param name="ct">Token to observe for cancellation.</param>
    /// <returns>A task that completes when the setup delegate finishes.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct) => _setup(provider, ct);
}
