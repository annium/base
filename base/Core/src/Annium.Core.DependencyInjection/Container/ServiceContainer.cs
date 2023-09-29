using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders;
using Annium.Core.DependencyInjection.Internal.Container;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public class ServiceContainer : IServiceContainer
{
    public int Count => Collection.Count;
    public IServiceCollection Collection { get; }
    public event Action<IServiceProvider> OnBuild = delegate { };

    public ServiceContainer() : this(new ServiceCollection())
    {
    }

    public ServiceContainer(IServiceCollection collection)
    {
        Collection = collection;
        AddInternalTypes();
    }

    public IServiceContainer Add(IServiceDescriptor descriptor)
    {
        Register(descriptor);

        return this;
    }

    public IBulkRegistrationBuilderBase Add(IEnumerable<Type> types) =>
        new BulkRegistrationBuilder(this, types, new Registrar(Register));

    public IFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object> factory) =>
        new FactoryRegistrationBuilder(this, type, factory, new Registrar(Register));

    public IInstanceRegistrationBuilderBase Add<T>(T instance) where T : class =>
        new InstanceRegistrationBuilder(this, typeof(T), instance, new Registrar(Register));

    public ISingleRegistrationBuilderBase Add(Type type) =>
        new SingleRegistrationBuilder(this, type, new Registrar(Register));

    public IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory)
        where T : class
        => Add(typeof(T), factory);

    public ISingleRegistrationBuilderBase Add<TService, TImplementation>()
        where TImplementation : TService
        => Add(typeof(TImplementation)).As<TService>();

    public ISingleRegistrationBuilderBase Add<TImplementationType>() =>
        Add(typeof(TImplementationType));

    public IServiceContainer Clone()
    {
        var clone = new ServiceContainer();

        foreach (var descriptor in this)
            clone.Add(descriptor);

        return clone;
    }

    public bool Contains(IServiceDescriptor descriptor)
    {
        var lifetime = (Microsoft.Extensions.DependencyInjection.ServiceLifetime)descriptor.Lifetime;

        return descriptor switch
        {
            ITypeServiceDescriptor d => Collection
                .Any(x =>
                    x.Lifetime == lifetime &&
                    x.ServiceType == d.ServiceType &&
                    x.ImplementationType == d.ImplementationType
                ),
            IFactoryServiceDescriptor d => Collection
                .Any(x =>
                    x.Lifetime == lifetime &&
                    x.ServiceType == d.ServiceType &&
                    x.ImplementationFactory?.Method == d.ImplementationFactory.Method &&
                    x.ImplementationFactory?.Target == d.ImplementationFactory.Target
                ),
            IInstanceServiceDescriptor d => Collection
                .Any(x =>
                    x.Lifetime == lifetime &&
                    x.ServiceType == d.ServiceType &&
                    x.ImplementationInstance == d.ImplementationInstance
                ),
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported")
        };
    }

    public ServiceProvider BuildServiceProvider()
    {
        var sp = Collection.BuildServiceProvider();
        OnBuild.Invoke(sp);

        return sp;
    }

    public IEnumerator<IServiceDescriptor> GetEnumerator() => Collection.Select(ServiceDescriptor.From).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

    private void Register(IServiceDescriptor item)
    {
        if (Contains(item))
        {
            // this.Trace("skip {item}, is already registered", item.ToReadableString());

            return;
        }

        // this.Trace("add {item}", item.ToReadableString());
        Collection.Add(item.ToMicrosoft());
    }

    private void AddInternalTypes()
    {
        Register(ServiceDescriptor.Type(typeof(IIndex<,>), typeof(Index<,>), ServiceLifetime.Transient));
    }
}