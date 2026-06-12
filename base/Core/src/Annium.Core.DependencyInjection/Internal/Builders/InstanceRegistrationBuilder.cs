using System;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for instance-based service registrations.
/// </summary>
internal class InstanceRegistrationBuilder : RegistrationBuilderBase, IInstanceRegistrationBuilderBase
{
    /// <summary>
    /// The type being registered.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// The instance to register.
    /// </summary>
    private readonly object _instance;

    /// <summary>
    /// Initializes a new instance of the InstanceRegistrationBuilder class.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="type">The type being registered.</param>
    /// <param name="instance">The instance to register.</param>
    /// <param name="registrar">The registrar for handling registrations.</param>
    public InstanceRegistrationBuilder(IServiceContainer container, Type type, object instance, Registrar registrar)
        : base(container, registrar)
    {
        _type = type;
        _instance = instance;
    }

    /// <summary>
    /// Registers the instance as its own type.
    /// </summary>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsSelf()
    {
        Track(new InstanceRegistration(_type, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase As(Type serviceType)
    {
        Track(new InstanceRegistration(serviceType, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as all interfaces implemented by its type.
    /// </summary>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsInterfaces()
    {
        Track(_type.GetInterfaces().Select(x => new InstanceRegistration(x, _instance)));
        return this;
    }

    /// <summary>
    /// Registers the instance as its own type with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsKeyedSelf(object key)
    {
        Track(new KeyedInstanceRegistration(_type, key, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as the specified service type with the specified key.
    /// </summary>
    /// <param name="serviceType">The service type to register as.</param>
    /// <param name="key">The key to associate with the service.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key)
    {
        Track(new KeyedInstanceRegistration(serviceType, key, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as each of its implemented interfaces with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with each interface registration.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsKeyedInterfaces(object key)
    {
        Track(_type.GetInterfaces().Select(x => new KeyedInstanceRegistration(x, key, _instance)));
        return this;
    }

    /// <summary>
    /// Registers the instance as a factory that returns itself.
    /// </summary>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsSelfFactory()
    {
        Track(new InstanceFactoryRegistration(_type, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as a factory that returns the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsFactory(Type serviceType)
    {
        Track(new InstanceFactoryRegistration(serviceType, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as a keyed factory that returns itself.
    /// </summary>
    /// <param name="key">The key to associate with the factory.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key)
    {
        Track(new KeyedInstanceFactoryRegistration(_type, key, _instance));
        return this;
    }

    /// <summary>
    /// Registers the instance as a keyed factory that returns the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <param name="key">The key to associate with the factory.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    public IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key)
    {
        Track(new KeyedInstanceFactoryRegistration(serviceType, key, _instance));
        return this;
    }

    /// <summary>
    /// Completes the registration with singleton lifetime.
    /// </summary>
    /// <returns>The service container.</returns>
    public IServiceContainer Singleton()
    {
        if (!RegistrationsInitiated)
            throw new InvalidOperationException(NoRegistrationTargetsMessage);

        Registrar.Register(Registrations, ServiceLifetime.Singleton);

        return Container;
    }
}
