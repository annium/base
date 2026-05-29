using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for keyed factory registration builder.
/// </summary>
public interface IKeyedFactoryRegistrationBuilderBase : IRegistrationBuilderLifetime
{
    /// <summary>
    /// Registers the type factory as a factory of the type itself with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Registers the type factory as a factory of the given service type with the given key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Registers the type factory as a factory of each interface implemented by the type with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key);

    /// <summary>
    /// Registers the user keyed factory as a keyed <c>Func&lt;T&gt;</c> for the registration's
    /// original type; each call to the resolved <c>Func&lt;T&gt;</c> invokes the user factory with
    /// the resolving <see cref="IServiceProvider"/> and the matched key.
    /// </summary>
    /// <param name="key">The key associated with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>Builder.</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Registers the user keyed factory as a keyed <c>Func&lt;TService&gt;</c> for the specified
    /// service type. Same use case as <see cref="AsKeyedSelfFactory"/> but exposed under a named
    /// service type.
    /// </summary>
    /// <param name="serviceType">The service type the <c>Func&lt;T&gt;</c> wraps.</param>
    /// <param name="key">The key associated with the <c>Func&lt;T&gt;</c> registration.</param>
    /// <returns>Builder.</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);
}
