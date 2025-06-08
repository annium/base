using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Implementation of the service container that manages service registrations.
/// </summary>
public class ServiceContainer : IServiceContainer
{
    /// <summary>
    /// Gets the number of service descriptors in the container.
    /// </summary>
    public int Count => Collection.Count;

    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Collection { get; }

    /// <summary>
    /// Event raised when the service provider is built.
    /// </summary>
    public event Action<IServiceProvider> OnBuild = delegate { };

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceContainer"/> class with a new service collection.
    /// </summary>
    public ServiceContainer()
        : this(new ServiceCollection()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceContainer"/> class with the specified service collection.
    /// </summary>
    /// <param name="collection">The service collection to use.</param>
    public ServiceContainer(IServiceCollection collection)
    {
        Collection = collection;
    }

    /// <summary>
    /// Register manually created service descriptor
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    /// <returns>container</returns>
    public IServiceContainer Add(IServiceDescriptor descriptor)
    {
        Register(descriptor);

        return this;
    }

    /// <summary>
    /// Register multiple types at once
    /// </summary>
    /// <param name="types">types</param>
    /// <returns>bulk registration builder</returns>
    public IBulkRegistrationBuilderBase Add(IEnumerable<Type> types) =>
        new BulkRegistrationBuilder(this, types, new Registrar(Register));

    /// <summary>
    /// Register type factory
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="factory">factory</param>
    /// <returns>factory registration builder</returns>
    public IFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object> factory) =>
        new FactoryRegistrationBuilder(this, type, factory, new Registrar(Register));

    /// <summary>
    /// Register type factory
    /// </summary>
    /// <param name="factory">factory</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>factory registration builder</returns>
    public IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory)
        where T : class
    {
        return Add(typeof(T), factory);
    }

    /// <summary>
    /// Register keyed type factory
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="factory">factory</param>
    /// <returns>keyed factory registration builder</returns>
    public IKeyedFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object, object> factory) =>
        new KeyedFactoryRegistrationBuilder(this, type, factory, new Registrar(Register));

    /// <summary>
    /// Register keyed type factory
    /// </summary>
    /// <param name="factory">factory</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>keyed factory registration builder</returns>
    public IKeyedFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, object, T> factory)
        where T : class
    {
        return Add(typeof(T), factory);
    }

    /// <summary>
    /// Register instance
    /// </summary>
    /// <param name="instance">instance</param>
    /// <typeparam name="T">type of instance</typeparam>
    /// <returns>instance registration builder</returns>
    public IInstanceRegistrationBuilderBase Add<T>(T instance)
        where T : class => new InstanceRegistrationBuilder(this, typeof(T), instance, new Registrar(Register));

    /// <summary>
    /// Register type
    /// </summary>
    /// <param name="type">type</param>
    /// <returns>type registration builder</returns>
    public ISingleRegistrationBuilderBase Add(Type type) =>
        new SingleRegistrationBuilder(this, type, new Registrar(Register));

    /// <summary>
    /// Clone existing container
    /// </summary>
    /// <returns>container clone</returns>
    public IServiceContainer Clone()
    {
        var clone = new ServiceContainer();

        foreach (var descriptor in this)
            clone.Add(descriptor);

        return clone;
    }

    /// <summary>
    /// Check whether given descriptor is registered in collection
    /// </summary>
    /// <param name="descriptor">descriptor to find</param>
    /// <returns>whether given descriptor is registered in collection</returns>
    /// <exception cref="NotSupportedException"></exception>
    public bool Contains(IServiceDescriptor descriptor)
    {
        var lifetime = (Microsoft.Extensions.DependencyInjection.ServiceLifetime)descriptor.Lifetime;

        return descriptor switch
        {
            ITypeServiceDescriptor d => Collection.Any(x =>
                !x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ImplementationType == d.ImplementationType
            ),
            IFactoryServiceDescriptor d => Collection.Any(x =>
                !x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ImplementationFactory?.Method == d.ImplementationFactory.Method
                && x.ImplementationFactory?.Target == d.ImplementationFactory.Target
            ),
            IInstanceServiceDescriptor d => Collection.Any(x =>
                !x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ImplementationInstance == d.ImplementationInstance
            ),
            IKeyedTypeServiceDescriptor d => Collection.Any(x =>
                x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ServiceKey == d.Key
                && x.KeyedImplementationType == d.ImplementationType
            ),
            IKeyedFactoryServiceDescriptor d => Collection.Any(x =>
                x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ServiceKey == d.Key
                && x.KeyedImplementationFactory?.Method == d.ImplementationFactory.Method
                && x.KeyedImplementationFactory?.Target == d.ImplementationFactory.Target
            ),
            IKeyedInstanceServiceDescriptor d => Collection.Any(x =>
                x.IsKeyedService
                && x.Lifetime == lifetime
                && x.ServiceType == d.ServiceType
                && x.ServiceKey == d.Key
                && x.KeyedImplementationInstance == d.ImplementationInstance
            ),
            _ => throw new NotSupportedException($"{descriptor.GetType().FriendlyName()} is not supported"),
        };
    }

    /// <summary>
    /// Build service provider
    /// </summary>
    /// <returns>The built service provider</returns>
    public ServiceProvider BuildServiceProvider()
    {
        var sp = Collection.BuildServiceProvider();
        OnBuild.Invoke(sp);

        return sp;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the service descriptors.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the service descriptors.</returns>
    public IEnumerator<IServiceDescriptor> GetEnumerator() => Collection.Select(ServiceDescriptor.From).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the service descriptors.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the service descriptors.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Collection.GetEnumerator();

    /// <summary>
    /// Registers a service descriptor in the container.
    /// </summary>
    /// <param name="item">The service descriptor to register.</param>
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
}
