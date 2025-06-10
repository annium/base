using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for single type service registrations
/// </summary>
internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
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
    /// The registrar for handling service registrations
    /// </summary>
    private readonly Registrar _registrar;

    /// <summary>
    /// The collection of registrations to be processed
    /// </summary>
    private readonly RegistrationsCollection _registrations = new();

    /// <summary>
    /// Initializes a new instance of the SingleRegistrationBuilder class
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type being registered</param>
    /// <param name="registrar">The registrar for handling registrations</param>
    public SingleRegistrationBuilder(IServiceContainer container, Type type, Registrar registrar)
    {
        _container = container;
        _type = type;
        _registrar = registrar;
    }

    /// <summary>
    /// Registers the type as itself
    /// </summary>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsSelf()
    {
        return WithRegistration(new TypeRegistration(_type, _type));
    }

    /// <summary>
    /// Registers the type as the specified service type
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase As(Type serviceType)
    {
        return WithRegistration(new TypeRegistration(serviceType, _type));
    }

    /// <summary>
    /// Registers the type as all interfaces it implements
    /// </summary>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsInterfaces()
    {
        return WithRegistrations(_type.GetInterfaces().Select(x => new TypeRegistration(x, _type)));
    }

    /// <summary>
    /// Registers the type as itself with the specified key
    /// </summary>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsKeyedSelf(object key)
    {
        return WithRegistration(new KeyedTypeRegistration(_type, key, _type));
    }

    /// <summary>
    /// Registers the type as the specified service type with the specified key
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsKeyed(Type serviceType, object key)
    {
        return WithRegistration(new KeyedTypeRegistration(serviceType, key, _type));
    }

    /// <summary>
    /// Registers the type as a factory that returns itself
    /// </summary>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsSelfFactory()
    {
        return WithRegistration(new TypeFactoryRegistration(_type, _type));
    }

    /// <summary>
    /// Registers the type as a factory that returns the specified service type
    /// </summary>
    /// <param name="serviceType">The service type the factory should return</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsFactory(Type serviceType)
    {
        return WithRegistration(new TypeFactoryRegistration(serviceType, _type));
    }

    /// <summary>
    /// Registers the type as a keyed factory that returns itself
    /// </summary>
    /// <param name="key">The key to associate with the factory</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsKeyedSelfFactory(object key)
    {
        return WithRegistration(new KeyedTypeFactoryRegistration(_type, key, _type));
    }

    /// <summary>
    /// Registers the type as a keyed factory that returns the specified service type
    /// </summary>
    /// <param name="serviceType">The service type the factory should return</param>
    /// <param name="key">The key to associate with the factory</param>
    /// <returns>The single registration builder for method chaining</returns>
    public ISingleRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key)
    {
        return WithRegistration(new KeyedTypeFactoryRegistration(serviceType, key, _type));
    }

    /// <summary>
    /// Completes the registration with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime</param>
    /// <returns>The service container</returns>
    public IServiceContainer In(ServiceLifetime lifetime)
    {
        _registrations.Add(new TypeRegistration(_type, _type));
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
    /// <returns>The single registration builder for method chaining</returns>
    private ISingleRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    /// <summary>
    /// Adds a single registration to the collection
    /// </summary>
    /// <param name="registration">The registration to add</param>
    /// <returns>The single registration builder for method chaining</returns>
    private ISingleRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}
