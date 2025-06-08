using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for keyed factory-based service registrations
/// </summary>
internal class KeyedFactoryRegistrationBuilder : IKeyedFactoryRegistrationBuilderBase
{
    /// <summary>
    /// The service container instance
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The type being registered
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// The keyed factory function to create instances
    /// </summary>
    private readonly Func<IServiceProvider, object, object> _factory;

    /// <summary>
    /// The registrar for handling service registrations
    /// </summary>
    private readonly Registrar _registrar;

    /// <summary>
    /// The collection of registrations to be processed
    /// </summary>
    private readonly RegistrationsCollection _registrations = new();

    /// <summary>
    /// Initializes a new instance of the KeyedFactoryRegistrationBuilder class
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type being registered</param>
    /// <param name="factory">The keyed factory function to create instances</param>
    /// <param name="registrar">The registrar for handling registrations</param>
    public KeyedFactoryRegistrationBuilder(
        IServiceContainer container,
        Type type,
        Func<IServiceProvider, object, object> factory,
        Registrar registrar
    )
    {
        _container = container;
        _type = type;
        _factory = factory;
        _registrar = registrar;
    }

    /// <summary>
    /// Registers the keyed factory as its own type with the specified key
    /// </summary>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The keyed factory registration builder for method chaining</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key) =>
        WithRegistration(new KeyedFactoryRegistration(_type, key, _factory));

    /// <summary>
    /// Registers the keyed factory as the specified service type with the specified key
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The keyed factory registration builder for method chaining</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key) =>
        WithRegistration(new KeyedFactoryRegistration(serviceType, key, _factory));

    /// <summary>
    /// Registers the keyed factory as all interfaces implemented by its type with the specified key
    /// </summary>
    /// <param name="key">The key to associate with the services</param>
    /// <returns>The keyed factory registration builder for method chaining</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key) =>
        WithRegistrations(_type.GetInterfaces().Select(x => new KeyedFactoryRegistration(x, key, _factory)));

    /// <summary>
    /// Completes the registration with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime</param>
    /// <returns>The service container</returns>
    public IServiceContainer In(ServiceLifetime lifetime)
    {
        _registrar.Register(_registrations, lifetime);

        return _container;
    }

    /// <summary>
    /// Completes the registration with scoped lifetime
    /// </summary>
    /// <returns>The service container</returns>
    public IServiceContainer Scoped() => In(ServiceLifetime.Scoped);

    /// <summary>
    /// Completes the registration with singleton lifetime
    /// </summary>
    /// <returns>The service container</returns>
    public IServiceContainer Singleton() => In(ServiceLifetime.Singleton);

    /// <summary>
    /// Completes the registration with transient lifetime
    /// </summary>
    /// <returns>The service container</returns>
    public IServiceContainer Transient() => In(ServiceLifetime.Transient);

    /// <summary>
    /// Adds multiple registrations to the collection
    /// </summary>
    /// <param name="registrations">The registrations to add</param>
    /// <returns>The keyed factory registration builder for method chaining</returns>
    private IKeyedFactoryRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    /// <summary>
    /// Adds a single registration to the collection
    /// </summary>
    /// <param name="registration">The registration to add</param>
    /// <returns>The keyed factory registration builder for method chaining</returns>
    private IKeyedFactoryRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}
