using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Descriptors;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Container;

/// <summary>
/// Interface for service container that manages service registrations.
/// </summary>
public interface IServiceContainer : IEnumerable<IServiceDescriptor>
{
    /// <summary>
    /// Gets the number of service descriptors in the container.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    IServiceCollection Collection { get; }

    /// <summary>
    /// Event raised when the service provider is built.
    /// </summary>
    event Action<IServiceProvider> OnBuild;

    /// <summary>
    /// Register manually created service descriptor
    /// </summary>
    /// <param name="descriptor">descriptor</param>
    /// <returns>container</returns>
    IServiceContainer Add(IServiceDescriptor descriptor);

    /// <summary>
    /// Register multiple types at once
    /// </summary>
    /// <param name="types">types</param>
    /// <returns>bulk registration builder</returns>
    IBulkRegistrationBuilderBase Add(IEnumerable<Type> types);

    /// <summary>
    /// Register type factory
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="factory">factory</param>
    /// <returns>factory registration builder</returns>
    IFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object> factory);

    /// <summary>
    /// Register type factory
    /// </summary>
    /// <param name="factory">factory</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>factory registration builder</returns>
    IFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, T> factory)
        where T : class;

    /// <summary>
    /// Register keyed type factory
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="factory">factory</param>
    /// <returns>keyed factory registration builder</returns>
    IKeyedFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object, object> factory);

    /// <summary>
    /// Register keyed type factory
    /// </summary>
    /// <param name="factory">factory</param>
    /// <typeparam name="T">type</typeparam>
    /// <returns>keyed factory registration builder</returns>
    IKeyedFactoryRegistrationBuilderBase Add<T>(Func<IServiceProvider, object, T> factory)
        where T : class;

    /// <summary>
    /// Register instance
    /// </summary>
    /// <param name="instance">instance</param>
    /// <typeparam name="T">type of instance</typeparam>
    /// <returns>instance registration builder</returns>
    IInstanceRegistrationBuilderBase Add<T>(T instance)
        where T : class;

    /// <summary>
    /// Register type
    /// </summary>
    /// <param name="type">type</param>
    /// <returns>type registration builder</returns>
    ISingleRegistrationBuilderBase Add(Type type);

    /// <summary>
    /// Clone existing container
    /// </summary>
    /// <returns>container clone</returns>
    IServiceContainer Clone();

    /// <summary>
    /// Check whether given descriptor is registered in collection
    /// </summary>
    /// <param name="descriptor">descriptor to find</param>
    /// <returns>whether given descriptor is registered in collection</returns>
    /// <exception cref="NotSupportedException"></exception>
    bool Contains(IServiceDescriptor descriptor);

    /// <summary>
    /// Build service provider
    /// </summary>
    /// <returns>The built service provider</returns>
    ServiceProvider BuildServiceProvider();
}
