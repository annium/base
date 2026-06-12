using System;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for single type service registrations.
/// </summary>
internal class SingleRegistrationBuilder : RegistrationBuilderBase, ISingleRegistrationBuilderBase
{
    /// <summary>
    /// The type being registered.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// Whether the caller already registered the type as itself via <see cref="AsSelf"/>.
    /// Tracked so that <see cref="In"/>'s implicit self-registration (needed for factory-style
    /// descriptors produced by <c>As</c>/<c>AsInterfaces</c>/<c>AsKeyed*</c> to resolve the
    /// implementation type) does not double-register when the caller already asked for it.
    /// </summary>
    private bool _selfAdded;

    /// <summary>
    /// Initializes a new instance of the SingleRegistrationBuilder class.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="type">The type being registered.</param>
    /// <param name="registrar">The registrar for handling registrations.</param>
    public SingleRegistrationBuilder(IServiceContainer container, Type type, Registrar registrar)
        : base(container, registrar)
    {
        _type = type;
    }

    /// <summary>
    /// Registers the type as itself.
    /// </summary>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsSelf()
    {
        _selfAdded = true;
        Track(new TypeRegistration(_type, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase As(Type serviceType)
    {
        Track(new TypeRegistration(serviceType, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as all interfaces it implements.
    /// </summary>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsInterfaces()
    {
        Track(_type.GetInterfaces().Select(x => new TypeRegistration(x, _type)));
        return this;
    }

    /// <summary>
    /// Registers the type as itself with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsKeyedSelf(object key)
    {
        Track(new KeyedTypeRegistration(_type, key, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as the specified service type with the specified key.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsKeyed(Type serviceType, object key)
    {
        Track(new KeyedTypeRegistration(serviceType, key, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as each of its implemented interfaces with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with each interface registration.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsKeyedInterfaces(object key)
    {
        Track(_type.GetInterfaces().Select(x => new KeyedTypeRegistration(x, key, _type)));
        return this;
    }

    /// <summary>
    /// Registers the type as a factory that returns itself.
    /// </summary>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsSelfFactory()
    {
        Track(new TypeFactoryRegistration(_type, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as a factory that returns the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsFactory(Type serviceType)
    {
        Track(new TypeFactoryRegistration(serviceType, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as a keyed factory that returns itself.
    /// </summary>
    /// <param name="key">The key to associate with the factory.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsKeyedSelfFactory(object key)
    {
        Track(new KeyedTypeFactoryRegistration(_type, key, _type));
        return this;
    }

    /// <summary>
    /// Registers the type as a keyed factory that returns the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <param name="key">The key to associate with the factory.</param>
    /// <returns>The single registration builder for method chaining.</returns>
    public ISingleRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key)
    {
        Track(new KeyedTypeFactoryRegistration(serviceType, key, _type));
        return this;
    }

    /// <summary>
    /// Completes the registration with the specified lifetime.
    /// <para>
    /// The implementation type is implicitly registered as itself unless the caller already
    /// did so via <see cref="AsSelf"/>. Factory-style descriptors produced by <c>As</c>,
    /// <c>AsInterfaces</c>, <c>AsKeyed*</c> resolve the implementation through the service
    /// provider, so it must be registered as itself for those descriptors to resolve.
    /// </para>
    /// </summary>
    /// <param name="lifetime">The service lifetime.</param>
    /// <returns>The service container.</returns>
    public IServiceContainer In(ServiceLifetime lifetime)
    {
        if (!RegistrationsInitiated)
            throw new InvalidOperationException(NoRegistrationTargetsMessage);

        if (!_selfAdded)
            Registrations.Add(new TypeRegistration(_type, _type));
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
