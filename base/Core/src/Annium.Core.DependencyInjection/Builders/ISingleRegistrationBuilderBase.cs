using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for single registration builder.
/// </summary>
public interface ISingleRegistrationBuilderBase : ISingleRegistrationBuilderLifetime
{
    /// <summary>
    /// Register type as self
    /// </summary>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Register type as given service type
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Register type as its interfaces
    /// </summary>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Register type as self with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Register type as given service type with given key
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Register type as self factory
    /// </summary>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Register type as factory of given service type
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Register type as self factory with given key
    /// </summary>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Register type as factory of given service type with given key
    /// </summary>
    /// <param name="serviceType">The service type to register</param>
    /// <param name="key">The key for registration</param>
    /// <returns>builder</returns>
    ISingleRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);
}
