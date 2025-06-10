using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Core.DependencyInjection.Builders;

/// <summary>
/// Base interface for factory registration builder.
/// </summary>
public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
{
    /// <summary>
    /// Register type factory as factory of type itself
    /// </summary>
    /// <returns>builder</returns>
    IFactoryRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Register type factory as factory of given service type
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <returns>builder</returns>
    IFactoryRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Register type factory as factory of type interfaces
    /// </summary>
    /// <returns>builder</returns>
    IFactoryRegistrationBuilderBase AsInterfaces();
}

/// <summary>
/// Interface for factory registration builder lifetime operations.
/// </summary>
public interface IFactoryRegistrationBuilderLifetime
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
