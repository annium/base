using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public class DynamicServicePack : ServicePackBase
{
    private Action<IServiceContainer> _configure = delegate { };
    private Action<IServiceContainer, IServiceProvider> _register = delegate { };
    private Action<IServiceProvider> _setup = delegate { };

    public DynamicServicePack Configure(Action<IServiceContainer> configure)
    {
        _configure = configure;
        return this;
    }

    public DynamicServicePack Register(Action<IServiceContainer, IServiceProvider> register)
    {
        _register = register;
        return this;
    }

    public DynamicServicePack Setup(Action<IServiceProvider> setup)
    {
        _setup = setup;
        return this;
    }

    public override void Configure(IServiceContainer container)
        => _configure(container);

    public override void Register(IServiceContainer container, IServiceProvider provider)
        => _register(container, provider);

    public override void Setup(IServiceProvider provider)
        => _setup(provider);
}