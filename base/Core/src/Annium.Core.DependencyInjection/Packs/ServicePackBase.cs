using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public abstract class ServicePackBase
{
    private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

    public void Add<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        _packs.Add(new TServicePack());
    }

    public virtual void Configure(IServiceContainer container)
    {
    }

    public virtual void Register(IServiceContainer container, IServiceProvider provider)
    {
    }

    public virtual void Setup(IServiceProvider provider)
    {
    }

    internal void InternalConfigure(IServiceContainer container)
    {
        foreach (var pack in _packs)
            pack.InternalConfigure(container);

        Configure(container);
    }

    internal void InternalRegister(IServiceContainer container, IServiceProvider provider)
    {
        foreach (var pack in _packs)
            pack.InternalRegister(container, provider);

        Register(container, provider);
    }

    internal void InternalSetup(IServiceProvider provider)
    {
        foreach (var pack in _packs)
            pack.InternalSetup(provider);

        Setup(provider);
    }
}