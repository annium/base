using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IInstanceRegistrationBuilderBase
{
    /// <summary>
    /// Register instance as self
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsSelf();

    /// <summary>
    /// Register instance as given service type
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase As(Type serviceType);

    /// <summary>
    /// Register instance as it's interfaces
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsInterfaces();

    /// <summary>
    /// Register instance as self with given key
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelf(object key);

    /// <summary>
    /// Register instance as given service type with given key
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyed(Type serviceType, object key);

    /// <summary>
    /// Register instance as self factory
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsSelfFactory();

    /// <summary>
    /// Register instance as factory of given service type
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsFactory(Type serviceType);

    /// <summary>
    /// Register instance as self factory with given key
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedSelfFactory(object key);

    /// <summary>
    /// Register instance as factory of given service type with given key
    /// </summary>
    /// <returns>builder</returns>
    IInstanceRegistrationBuilderBase AsKeyedFactory(Type serviceType, object key);

    IServiceContainer Singleton();
}
