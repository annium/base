using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions.Internal;
using Annium.Core.DependencyInjection;
using Annium.Reflection;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Extension methods for IServiceContainer to register configuration services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers a configuration instance in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configuration">The configuration instance to register</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddConfiguration<T>(this IServiceContainer container, T configuration)
        where T : class, new()
    {
        container.Add(configuration).AsSelf().Singleton();

        Register(container, typeof(T));

        return container;
    }

    /// <summary>
    /// Registers a configuration built from the provided async configuration function.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="configure">Async function to configure the configuration container. Receives the same cancellation token threaded into <see cref="ConfigurationContainerExtensions.BuildAsync"/>.</param>
    /// <param name="ct">Cancellation token forwarded to the configure delegate and to <see cref="ConfigurationContainerExtensions.BuildAsync"/>.</param>
    /// <returns>Task containing the service container for method chaining.</returns>
    public static async Task<IServiceContainer> AddConfigurationAsync<T>(
        this IServiceContainer container,
        Func<IConfigurationContainer, CancellationToken, Task> configure,
        CancellationToken ct = default
    )
        where T : class, new()
    {
        container.AddConfigurationBuilder();

        var cfgContainer = new ConfigurationContainer();
        await configure(cfgContainer, ct);
        await cfgContainer.BuildAsync(ct);

        container
            .Add(sp =>
            {
                var builder = sp.Resolve<IConfigurationBuilder>();
                builder.Add(cfgContainer.Get());

                return builder.Build<T>();
            })
            .AsSelf()
            .Singleton();

        Register(container, typeof(T));

        return container;
    }

    /// <summary>
    /// Registers a configuration built from the provided sync configuration action.
    /// </summary>
    /// <remarks>
    /// Ergonomic forwarder over the primary async overload. The supplied <paramref name="configure"/>
    /// action runs synchronously and does not observe <paramref name="ct"/>; the cancellation token is
    /// forwarded only into <see cref="ConfigurationContainerExtensions.BuildAsync"/>.
    /// </remarks>
    /// <param name="container">The service container.</param>
    /// <param name="configure">Action to configure the configuration container.</param>
    /// <param name="ct">Cancellation token forwarded to <see cref="ConfigurationContainerExtensions.BuildAsync"/>.</param>
    /// <returns>Task containing the service container for method chaining.</returns>
    public static Task<IServiceContainer> AddConfigurationAsync<T>(
        this IServiceContainer container,
        Action<IConfigurationContainer> configure,
        CancellationToken ct = default
    )
        where T : class, new() =>
        container.AddConfigurationAsync<T>(
            (cfg, _) =>
            {
                configure(cfg);
                return Task.CompletedTask;
            },
            ct
        );

    /// <summary>
    /// Registers the configuration builder in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    private static void AddConfigurationBuilder(this IServiceContainer container)
    {
        container.Add<IConfigurationBuilder, ConfigurationBuilder>().AsFactory<IConfigurationBuilder>().Transient();
    }

    /// <summary>
    /// Registers all nested properties of the specified type in the service container.
    /// Uses a visited-set to prevent infinite recursion on self-referential or
    /// mutually-referential config types (which would otherwise StackOverflow).
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type to register properties for</param>
    private static void Register(IServiceContainer container, Type type)
    {
        var visited = new HashSet<Type> { type };
        foreach (var property in GetRegisteredProperties(type))
            Register(container, type, property, visited);
    }

    /// <summary>
    /// Registers a specific property of a type in the service container, recursing into
    /// nested non-collection reference-type properties guarded by <paramref name="visited"/>.
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type containing the property</param>
    /// <param name="property">The property to register</param>
    /// <param name="visited">Set of property types already registered in this recursion — prevents cycles.</param>
    private static void Register(IServiceContainer container, Type type, PropertyInfo property, HashSet<Type> visited)
    {
        var propertyType = property.PropertyType;
        // Skip cycle: a property of an already-registered type would recurse forever.
        if (!visited.Add(propertyType))
            return;

        container.Add(propertyType, sp => property.GetValue(sp.Resolve(type)).NotNull()).AsSelf().Singleton();

        foreach (var prop in GetRegisteredProperties(propertyType))
            Register(container, propertyType, prop, visited);
    }

    /// <summary>
    /// Gets properties that should be registered for a type
    /// </summary>
    /// <param name="type">The type to get properties for</param>
    /// <returns>Collection of properties that should be registered</returns>
    private static IReadOnlyCollection<PropertyInfo> GetRegisteredProperties(Type type) =>
        type.GetProperties()
            .Where(x =>
                x is { CanRead: true, PropertyType: { IsEnum: false, IsValueType: false, IsPrimitive: false } }
                && !x.PropertyType.IsDerivedFrom(typeof(IEnumerable<>))
            )
            .ToArray();
}
