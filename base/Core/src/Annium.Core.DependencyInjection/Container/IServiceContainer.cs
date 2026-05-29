using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
    /// Event raised when <see cref="BuildServiceProvider"/> produces a provider.
    /// </summary>
    /// <remarks>
    /// Subscribers are not propagated when the container is cloned via <see cref="Clone"/> —
    /// re-attach handlers on the clone if post-build notification is needed there.
    /// </remarks>
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
    /// Clone existing container.
    /// </summary>
    /// <remarks>
    /// Only descriptors are copied. <see cref="OnBuild"/> subscribers are NOT propagated to the
    /// clone — callers that need post-build notification on the clone must re-attach handlers to it.
    /// </remarks>
    /// <returns>container clone</returns>
    IServiceContainer Clone();

    /// <summary>
    /// Check whether given descriptor is registered in collection.
    /// </summary>
    /// <remarks>
    /// For factory descriptors (<see cref="IFactoryServiceDescriptor"/>,
    /// <see cref="IKeyedFactoryServiceDescriptor"/>) equality is computed by delegate identity
    /// (<c>Method</c> + <c>Target</c>). This works for directly-supplied factory delegates
    /// (static methods, lambdas captured as variables) but does NOT identify factories produced
    /// by the fluent builder pipeline, because each builder invocation compiles a fresh expression
    /// lambda with a unique <c>Method</c> + <c>Target</c>. As a result calling <c>Contains</c>
    /// on a builder-produced factory descriptor against a previously-built sibling will return
    /// <see langword="false"/>, and the dedup guard in the registrar will not fire for repeated
    /// builder-path registrations. Callers that need idempotent registration through the builder
    /// path must avoid duplicate <c>Add(...).AsX(...).In(...)</c> chains themselves.
    /// </remarks>
    /// <param name="descriptor">descriptor to find</param>
    /// <returns>whether given descriptor is registered in collection</returns>
    /// <exception cref="NotSupportedException">Thrown when <paramref name="descriptor"/> is not one of the six recognised <see cref="IServiceDescriptor"/> subtypes.</exception>
    bool Contains(IServiceDescriptor descriptor);

    /// <summary>
    /// Build service provider
    /// </summary>
    /// <returns>The built service provider</returns>
    ServiceProvider BuildServiceProvider();
}
