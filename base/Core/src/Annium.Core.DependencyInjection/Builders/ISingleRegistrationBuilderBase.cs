using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for single registration builder.
/// </summary>
public interface ISingleRegistrationBuilderBase : ISingleRegistrationBuilderLifetime
{
    /// <summary>
    /// Registers the type as itself.
    /// </summary>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Registers the type as the given service type.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Registers the type as each of its implemented interfaces.
    /// </summary>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Registers the type as itself with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Registers the type as the given service type with the given key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Registers the type as a self factory.
    /// </summary>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Registers the type as a factory of the given service type.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Registers the type as a self factory with the given key.
    /// </summary>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Registers the type as a factory of the given service type with the given key.
    /// </summary>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>Builder.</returns>
    ISingleRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);
}
