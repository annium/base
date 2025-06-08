using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
using Annium.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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
    /// Registers a configuration built from the provided configuration action
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configure">Action to configure the configuration container</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddConfiguration<T>(
        this IServiceContainer container,
        Action<IConfigurationContainer> configure
    )
        where T : class, new()
    {
        container.AddConfigurationBuilder();

        var cfgContainer = new ConfigurationContainer();
        configure(cfgContainer);

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
    /// Registers a configuration built from the provided asynchronous configuration function
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configure">Async function to configure the configuration container</param>
    /// <returns>Task containing the service container for method chaining</returns>
    public static async Task<IServiceContainer> AddConfigurationAsync<T>(
        this IServiceContainer container,
        Func<IConfigurationContainer, Task> configure
    )
        where T : class, new()
    {
        container.AddConfigurationBuilder();

        var cfgContainer = new ConfigurationContainer();
        await configure(cfgContainer);

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
    /// Registers the configuration builder in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    private static void AddConfigurationBuilder(this IServiceContainer container)
    {
        container.Add<IConfigurationBuilder, ConfigurationBuilder>().AsFactory<IConfigurationBuilder>().Transient();
    }

    /// <summary>
    /// Registers all nested properties of the specified type in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type to register properties for</param>
    private static void Register(IServiceContainer container, Type type)
    {
        foreach (var property in GetRegisteredProperties(type))
            Register(container, type, property);
    }

    /// <summary>
    /// Registers a specific property of a type in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="type">The type containing the property</param>
    /// <param name="property">The property to register</param>
    private static void Register(IServiceContainer container, Type type, PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        container.Add(propertyType, sp => property.GetValue(sp.Resolve(type))!).AsSelf().Singleton();

        foreach (var prop in GetRegisteredProperties(propertyType))
            Register(container, propertyType, prop);
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
