using System;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Core.DependencyInjection.Builders;

/// <summary>
/// Base interface for instance registration builder.
/// </summary>
public interface IInstanceRegistrationBuilderBase
{
    /// <summary>
    /// Register instance as self
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Register instance as given service type
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Register instance as its interfaces
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Register instance as self with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Register instance as given service type with given key
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Register instance as self factory
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Register instance as factory of given service type
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Register instance as self factory with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Register instance as factory of given service type with given key
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);

    /// <summary>
    /// Sets the service lifetime to singleton for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Singleton();
}
