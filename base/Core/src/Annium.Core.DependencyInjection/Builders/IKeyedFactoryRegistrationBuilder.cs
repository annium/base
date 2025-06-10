using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Builders;

/// <summary>
/// Base interface for keyed factory registration builder.
/// </summary>
public interface IKeyedFactoryRegistrationBuilderBase : IKeyedFactoryRegistrationBuilderLifetime
{
    /// <summary>
    /// Register type factory as factory of type itself with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Register type factory as factory of given service type with given key
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Register type factory as factory of type interfaces with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key);
}

/// <summary>
/// Interface for keyed factory registration builder lifetime operations.
/// </summary>
public interface IKeyedFactoryRegistrationBuilderLifetime
{
    /// <summary>
    /// Sets the service lifetime for the registration.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use</param>
    /// <returns>The service container instance</returns>
    IServiceContainer In(ServiceLifetime lifetime);

    /// <summary>
    /// Sets the service lifetime to scoped for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Scoped();

    /// <summary>
    /// Sets the service lifetime to singleton for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Singleton();

    /// <summary>
    /// Sets the service lifetime to transient for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Transient();
}
