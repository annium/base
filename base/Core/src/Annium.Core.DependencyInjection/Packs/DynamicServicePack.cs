using System;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Core.DependencyInjection.Packs;

/// <summary>
/// A dynamic service pack that allows configuring services through delegate actions
/// </summary>
public class DynamicServicePack : ServicePackBase
{
    /// <summary>
    /// The action to configure services in the container
    /// </summary>
    private Action<IServiceContainer> _configure = delegate { };

    /// <summary>
    /// The action to register services in the container
    /// </summary>
    private Action<IServiceContainer, IServiceProvider> _register = delegate { };

    /// <summary>
    /// The action to setup services using the provider
    /// </summary>
    private Action<IServiceProvider> _setup = delegate { };

    /// <summary>
    /// Sets the configuration action for this service pack
    /// </summary>
    /// <param name="configure">The action to configure services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Configure(Action<IServiceContainer> configure)
    {
        _configure = configure;
        return this;
    }

    /// <summary>
    /// Sets the registration action for this service pack
    /// </summary>
    /// <param name="register">The action to register services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Register(Action<IServiceContainer, IServiceProvider> register)
    {
        _register = register;
        return this;
    }

    /// <summary>
    /// Sets the setup action for this service pack
    /// </summary>
    /// <param name="setup">The action to setup services</param>
    /// <returns>The current dynamic service pack instance</returns>
    public DynamicServicePack Setup(Action<IServiceProvider> setup)
    {
        _setup = setup;
        return this;
    }

    /// <summary>
    /// Configures the service container using the configured action
    /// </summary>
    /// <param name="container">The service container to configure</param>
    public override void Configure(IServiceContainer container) => _configure(container);

    /// <summary>
    /// Registers services in the container using the configured action
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <param name="provider">The service provider for resolving dependencies</param>
    public override void Register(IServiceContainer container, IServiceProvider provider) =>
        _register(container, provider);

    /// <summary>
    /// Sets up services using the configured action
    /// </summary>
    /// <param name="provider">The service provider to setup services with</param>
    public override void Setup(IServiceProvider provider) => _setup(provider);
}
