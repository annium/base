using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceContainer : IEnumerable<IServiceDescriptor>
{
    int Count { get; }
    IServiceCollection Collection { get; }
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
    /// Register keyed type factory
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="factory">factory</param>
    /// <returns>keyed factory registration builder</returns>
    IKeyedFactoryRegistrationBuilderBase Add(Type type, Func<IServiceProvider, object, object> factory);

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
    /// <returns></returns>
    ServiceProvider BuildServiceProvider();
}
