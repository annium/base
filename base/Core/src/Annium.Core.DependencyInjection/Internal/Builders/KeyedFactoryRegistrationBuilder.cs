using System;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for keyed factory-based service registrations.
/// </summary>
internal class KeyedFactoryRegistrationBuilder : RegistrationBuilderBase, IKeyedFactoryRegistrationBuilderBase
{
    /// <summary>
    /// The type being registered.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// The keyed factory function to create instances.
    /// </summary>
    private readonly Func<IServiceProvider, object, object> _factory;

    /// <summary>
    /// Initializes a new instance of the KeyedFactoryRegistrationBuilder class.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="type">The type being registered.</param>
    /// <param name="factory">The keyed factory function to create instances.</param>
    /// <param name="registrar">The registrar for handling registrations.</param>
    public KeyedFactoryRegistrationBuilder(
        IServiceContainer container,
        Type type,
        Func<IServiceProvider, object, object> factory,
        Registrar registrar
    )
        : base(container, registrar)
    {
        _type = type;
        _factory = factory;
    }

    /// <summary>
    /// Registers the keyed factory as its own type with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The keyed factory registration builder for method chaining.</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key)
    {
        Track(new KeyedFactoryRegistration(_type, key, _factory));
        return this;
    }

    /// <summary>
    /// Registers the keyed factory as the specified service type with the specified key.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The keyed factory registration builder for method chaining.</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key)
    {
        Track(new KeyedFactoryRegistration(serviceType, key, _factory));
        return this;
    }

    /// <summary>
    /// Registers the keyed factory as all interfaces implemented by its type with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the services.</param>
    /// <returns>The keyed factory registration builder for method chaining.</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key)
    {
        Track(_type.GetInterfaces().Select(x => new KeyedFactoryRegistration(x, key, _factory)));
        return this;
    }

    /// <summary>
    /// Registers the keyed factory as a keyed <c>Func&lt;T&gt;</c> for the registration's own type.
    /// </summary>
    /// <param name="key">The key to associate with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>The keyed factory registration builder for method chaining.</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedSelfFactory(object key)
    {
        Track(new KeyedFactoryFactoryRegistration(_type, key, _factory));
        return this;
    }

    /// <summary>
    /// Registers the keyed factory as a keyed <c>Func&lt;serviceType&gt;</c>.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <param name="key">The key to associate with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>The keyed factory registration builder for method chaining.</returns>
    public IKeyedFactoryRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key)
    {
        Track(new KeyedFactoryFactoryRegistration(serviceType, key, _factory));
        return this;
    }

    /// <summary>
    /// Completes the registration with the specified lifetime.
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service container.</returns>
    public IServiceContainer In(ServiceLifetime lifetime)
    {
        if (!RegistrationsInitiated)
            throw new InvalidOperationException(NoRegistrationTargetsMessage);

        Registrar.Register(Registrations, lifetime);

        return Container;
    }

    /// <summary>
    /// Completes the registration with scoped lifetime.
    /// </summary>
    /// <returns>The service container.</returns>
    public IServiceContainer Scoped() => In(ServiceLifetime.Scoped);

    /// <summary>
    /// Completes the registration with singleton lifetime.
    /// </summary>
    /// <returns>The service container.</returns>
    public IServiceContainer Singleton() => In(ServiceLifetime.Singleton);

    /// <summary>
    /// Completes the registration with transient lifetime.
    /// </summary>
    /// <returns>The service container.</returns>
    public IServiceContainer Transient() => In(ServiceLifetime.Transient);
}
