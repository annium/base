using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
