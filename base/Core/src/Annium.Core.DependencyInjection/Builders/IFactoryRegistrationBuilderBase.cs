using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for factory registration builder.
/// </summary>
public interface IFactoryRegistrationBuilderBase : IFactoryRegistrationBuilderLifetime
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
}
