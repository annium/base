using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for instance registration builder.
/// </summary>
public interface IInstanceRegistrationBuilderBase
{
    /// <summary>
    /// Registers the instance as itself.
    /// </summary>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Registers the instance as the given service type.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Registers the instance as each of its implemented interfaces.
    /// </summary>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Registers the instance as itself with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Registers the instance as the given service type with the given key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Registers the instance as each of its implemented interfaces with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IInstanceRegistrationBuilderBase AsKeyedInterfaces(object key);

    /// <summary>
    /// Registers the instance as a factory <c>Func&lt;T&gt;</c> that returns it on every call.
    /// Useful when a consumer depends on <c>Func&lt;T&gt;</c> (for example for lazy or deferred
    /// access) while the underlying value is a pre-built singleton instance.
    /// </summary>
    /// <returns>The instance registration builder for method chaining.</returns>
    IInstanceRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Registers the instance as a factory <c>Func&lt;TService&gt;</c> for the specified service
    /// type. Same use case as <see cref="AsSelfFactory"/> but resolved as the named service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    IInstanceRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Registers the instance as a keyed factory <c>Func&lt;T&gt;</c> that returns it on every
    /// call. Useful when keyed consumers depend on <c>Func&lt;T&gt;</c> for lazy or deferred
    /// access while the underlying value is a pre-built singleton instance.
    /// </summary>
    /// <param name="key">The key associated with the factory registration.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Registers the instance as a keyed factory <c>Func&lt;TService&gt;</c> for the specified
    /// service type. Same use case as <see cref="AsKeyedSelfFactory"/> but resolved as the named
    /// service type.
    /// </summary>
    /// <param name="serviceType">The service type the factory should return.</param>
    /// <param name="key">The key associated with the factory registration.</param>
    /// <returns>The instance registration builder for method chaining.</returns>
    IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);

    /// <summary>
    /// Sets the service lifetime to singleton for the registration.
    /// </summary>
    /// <returns>The service container instance.</returns>
    IServiceContainer Singleton();
}
