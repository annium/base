using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedFactoryRegistrationBuilderBase : IKeyedFactoryRegistrationBuilderLifetime
{
    /// <summary>
    /// Register type factory as factory of type itself with given key
    /// </summary>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Register type factory as factory of given service type with given key
    /// </summary>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Register type factory as factory of type interfaces with given key
    /// </summary>
    /// <returns>builder</returns>
    IKeyedFactoryRegistrationBuilderBase AsKeyedInterfaces(object key);
}

public interface IKeyedFactoryRegistrationBuilderLifetime
{
    IServiceContainer In(ServiceLifetime lifetime);
    IServiceContainer Scoped();
    IServiceContainer Singleton();
    IServiceContainer Transient();
}
