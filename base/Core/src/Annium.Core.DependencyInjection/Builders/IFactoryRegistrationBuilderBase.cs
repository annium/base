using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for factory registration builder.
/// </summary>
public interface IFactoryRegistrationBuilderBase : IRegistrationBuilderLifetime
{
    /// <summary>
    /// Registers the type factory as a factory of the type itself.
    /// </summary>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Registers the type factory as a factory of the given service type.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Registers the type factory as a factory of each interface implemented by the type.
    /// </summary>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Registers the user factory as a <c>Func&lt;T&gt;</c> for the registration's original type;
    /// each call to the resolved <c>Func&lt;T&gt;</c> invokes the user factory with the resolving
    /// <see cref="IServiceProvider"/>. Useful when consumers want deferred resolution while still
    /// going through the user-supplied factory logic.
    /// </summary>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Registers the user factory as a <c>Func&lt;TService&gt;</c> for the specified service type.
    /// Same use case as <see cref="AsSelfFactory"/> but exposed under a named service type.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Registers the user factory as its own type with the given key. The factory ignores the key.
    /// </summary>
    /// <param name="key">The key for the registration.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Registers the user factory as the given service type with the given key. The factory ignores the key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key for the registration.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Registers the user factory as each of its implemented interfaces with the given key. The factory ignores the key.
    /// </summary>
    /// <param name="key">The key for the registration.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsKeyedInterfaces(object key);

    /// <summary>
    /// Registers the user factory as a keyed <c>Func&lt;T&gt;</c> for the registration's own type.
    /// </summary>
    /// <param name="key">The key for the registration.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Registers the user factory as a keyed <c>Func&lt;TService&gt;</c> for the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <param name="key">The key for the registration.</param>
    /// <returns>Builder.</returns>
    IFactoryRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);
}
