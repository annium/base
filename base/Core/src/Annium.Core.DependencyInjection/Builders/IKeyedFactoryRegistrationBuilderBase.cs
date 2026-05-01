using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for keyed factory registration builder.
/// </summary>
public interface IKeyedFactoryRegistrationBuilderBase : IKeyedFactoryRegistrationBuilderLifetime
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
}
