using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
