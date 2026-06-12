using System;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for factory-based service registrations.
/// </summary>
internal class FactoryRegistrationBuilder : RegistrationBuilderBase, IFactoryRegistrationBuilderBase
{
    /// <summary>
    /// The type being registered.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// The factory function to create instances.
    /// </summary>
    private readonly Func<IServiceProvider, object> _factory;

    /// <summary>
    /// Initializes a new instance of the FactoryRegistrationBuilder class.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="type">The type being registered.</param>
    /// <param name="factory">The factory function to create instances.</param>
    /// <param name="registrar">The registrar for handling registrations.</param>
    public FactoryRegistrationBuilder(
        IServiceContainer container,
        Type type,
        Func<IServiceProvider, object> factory,
        Registrar registrar
    )
        : base(container, registrar)
    {
        _type = type;
        _factory = factory;
    }

    /// <summary>
    /// Registers the factory as its own type.
    /// </summary>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsSelf()
    {
        Track(new FactoryRegistration(_type, _factory));
        return this;
    }

    /// <summary>
    /// Registers the factory as the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase As(Type serviceType)
    {
        Track(new FactoryRegistration(serviceType, _factory));
        return this;
    }

    /// <summary>
    /// Registers the factory as all interfaces implemented by its type.
    /// </summary>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsInterfaces()
    {
        Track(_type.GetInterfaces().Select(x => new FactoryRegistration(x, _factory)));
        return this;
    }

    /// <summary>
    /// Registers the factory as a <c>Func&lt;T&gt;</c> for the registration's own type.
    /// </summary>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsSelfFactory()
    {
        Track(new FactoryFactoryRegistration(_type, _factory));
        return this;
    }

    /// <summary>
    /// Registers the factory as a <c>Func&lt;serviceType&gt;</c>.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsFactory(Type serviceType)
    {
        Track(new FactoryFactoryRegistration(serviceType, _factory));
        return this;
    }

    /// <summary>
    /// Registers the factory as its own type with the specified key. The factory ignores the key.
    /// </summary>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsKeyedSelf(object key)
    {
        Track(new KeyedFactoryRegistration(_type, key, IgnoreKey));
        return this;
    }

    /// <summary>
    /// Registers the factory as the specified service type with the specified key. The factory ignores the key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key)
    {
        Track(new KeyedFactoryRegistration(serviceType, key, IgnoreKey));
        return this;
    }

    /// <summary>
    /// Registers the factory as each of its implemented interfaces with the specified key. The factory ignores the key.
    /// </summary>
    /// <param name="key">The key to associate with each interface registration.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsKeyedInterfaces(object key)
    {
        Track(_type.GetInterfaces().Select(x => new KeyedFactoryRegistration(x, key, IgnoreKey)));
        return this;
    }

    /// <summary>
    /// Registers the factory as a keyed <c>Func&lt;T&gt;</c> for the registration's own type. The factory ignores the key.
    /// </summary>
    /// <param name="key">The key to associate with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsKeyedSelfFactory(object key)
    {
        Track(new KeyedFactoryFactoryRegistration(_type, key, IgnoreKey));
        return this;
    }

    /// <summary>
    /// Registers the factory as a keyed <c>Func&lt;serviceType&gt;</c>. The factory ignores the key.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <param name="key">The key to associate with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>The factory registration builder for method chaining.</returns>
    public IFactoryRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key)
    {
        Track(new KeyedFactoryFactoryRegistration(serviceType, key, IgnoreKey));
        return this;
    }

    /// <summary>
    /// Adapts a non-keyed factory to the keyed factory signature by discarding the key.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    /// <param name="_">The key (discarded).</param>
    /// <returns>The instance produced by the wrapped non-keyed factory.</returns>
    private object IgnoreKey(IServiceProvider sp, object _) => _factory(sp);

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
