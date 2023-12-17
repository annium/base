using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
    /// <returns>builder</returns>
    IFactoryRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Register type factory as factory of type interfaces
    /// </summary>
    /// <returns>builder</returns>
    IFactoryRegistrationBuilderBase AsInterfaces();
}

public interface IFactoryRegistrationBuilderLifetime
{
    IServiceContainer In(ServiceLifetime lifetime);
    IServiceContainer Scoped();
    IServiceContainer Singleton();
    IServiceContainer Transient();
}
