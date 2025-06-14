using IBuilderTarget = Annium.Core.DependencyInjection.IInstanceRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for instance registration builder.
/// </summary>
public static class InstanceRegistrationBuilderExtensions
{
    /// <summary>
    /// Registers the instance as the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderTarget As<T>(this IBuilderTarget builder) => builder.As(typeof(T));

    /// <summary>
    /// Registers the instance as the specified service type with a key.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderTarget AsKeyed<T>(this IBuilderTarget builder, object key) => builder.AsKeyed(typeof(T), key);

    /// <summary>
    /// Registers the instance as a factory for the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type to register as a factory.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) => builder.AsFactory(typeof(T));

    /// <summary>
    /// Registers the instance as a factory for the specified service type with a key.
    /// </summary>
    /// <typeparam name="T">The service type to register as a factory.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderTarget AsKeyedFactory<T>(this IBuilderTarget builder, object key) =>
        builder.AsKeyedFactory(typeof(T), key);
}
