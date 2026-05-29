using IBuilderBase = Annium.Core.DependencyInjection.IFactoryRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for factory registration builder.
/// </summary>
public static class FactoryRegistrationBuilderExtensions
{
    /// <summary>
    /// Registers the factory with the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderBase As<T>(this IBuilderBase builder) => builder.As(typeof(T));

    /// <summary>
    /// Registers the factory as a <c>Func&lt;T&gt;</c> for the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type the <c>Func&lt;T&gt;</c> wraps.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderBase AsFactory<T>(this IBuilderBase builder) => builder.AsFactory(typeof(T));

    /// <summary>
    /// Registers the factory as the specified service type with the given key.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderBase AsKeyed<T>(this IBuilderBase builder, object key) => builder.AsKeyed(typeof(T), key);

    /// <summary>
    /// Registers the factory as a keyed <c>Func&lt;T&gt;</c> for the specified service type.
    /// </summary>
    /// <typeparam name="T">The service type the <c>Func&lt;T&gt;</c> wraps.</typeparam>
    /// <param name="builder">The builder instance.</param>
    /// <param name="key">The key for registration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public static IBuilderBase AsKeyedFactory<T>(this IBuilderBase builder, object key) =>
        builder.AsKeyedFactory(typeof(T), key);
}
