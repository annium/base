using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders;

/// <summary>
/// Builder for instance-based service registrations
/// </summary>
internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
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
    /// The instance to register
    /// </summary>
    private readonly object _instance;

    /// <summary>
    /// The registrar for handling service registrations
    /// </summary>
    private readonly Registrar _registrar;

    /// <summary>
    /// The collection of registrations to be processed
    /// </summary>
    private readonly RegistrationsCollection _registrations = new();

    /// <summary>
    /// Initializes a new instance of the InstanceRegistrationBuilder class
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type being registered</param>
    /// <param name="instance">The instance to register</param>
    /// <param name="registrar">The registrar for handling registrations</param>
    public InstanceRegistrationBuilder(IServiceContainer container, Type type, object instance, Registrar registrar)
    {
        _container = container;
        _type = type;
        _instance = instance;
        _registrar = registrar;
    }

    /// <summary>
    /// Registers the instance as its own type
    /// </summary>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsSelf() => WithRegistration(new InstanceRegistration(_type, _instance));

    /// <summary>
    /// Registers the instance as the specified service type
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase As(Type serviceType) =>
        WithRegistration(new InstanceRegistration(serviceType, _instance));

    /// <summary>
    /// Registers the instance as all interfaces implemented by its type
    /// </summary>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsInterfaces() =>
        WithRegistrations(_type.GetInterfaces().Select(x => new InstanceRegistration(x, _instance)));

    /// <summary>
    /// Registers the instance as its own type with the specified key
    /// </summary>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsKeyedSelf(object key) =>
        WithRegistration(new KeyedInstanceRegistration(_type, key, _instance));

    /// <summary>
    /// Registers the instance as the specified service type with the specified key
    /// </summary>
    /// <param name="serviceType">The service type to register as</param>
    /// <param name="key">The key to associate with the service</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key) =>
        WithRegistration(new KeyedInstanceRegistration(serviceType, key, _instance));

    /// <summary>
    /// Registers the instance as a factory that returns itself
    /// </summary>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsSelfFactory() =>
        WithRegistration(new InstanceFactoryRegistration(_type, _instance));

    /// <summary>
    /// Registers the instance as a factory that returns the specified service type
    /// </summary>
    /// <param name="serviceType">The service type the factory should return</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsFactory(Type serviceType) =>
        WithRegistration(new InstanceFactoryRegistration(serviceType, _instance));

    /// <summary>
    /// Registers the instance as a keyed factory that returns itself
    /// </summary>
    /// <param name="key">The key to associate with the factory</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key) =>
        WithRegistration(new KeyedInstanceFactoryRegistration(_type, key, _instance));

    /// <summary>
    /// Registers the instance as a keyed factory that returns the specified service type
    /// </summary>
    /// <param name="serviceType">The service type the factory should return</param>
    /// <param name="key">The key to associate with the factory</param>
    /// <returns>The instance registration builder for method chaining</returns>
    public IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key) =>
        WithRegistration(new KeyedInstanceFactoryRegistration(serviceType, key, _instance));

    /// <summary>
    /// Completes the registration with singleton lifetime
    /// </summary>
    /// <returns>The service container</returns>
    public IServiceContainer Singleton()
    {
        _registrar.Register(_registrations, ServiceLifetime.Singleton);

        return _container;
    }

    /// <summary>
    /// Adds multiple registrations to the collection
    /// </summary>
    /// <param name="registrations">The registrations to add</param>
    /// <returns>The instance registration builder for method chaining</returns>
    private IInstanceRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
    {
        _registrations.Init();
        _registrations.AddRange(registrations);

        return this;
    }

    /// <summary>
    /// Adds a single registration to the collection
    /// </summary>
    /// <param name="registration">The registration to add</param>
    /// <returns>The instance registration builder for method chaining</returns>
    private IInstanceRegistrationBuilderBase WithRegistration(IRegistration registration)
    {
        _registrations.Init();
        _registrations.Add(registration);

        return this;
    }
}
