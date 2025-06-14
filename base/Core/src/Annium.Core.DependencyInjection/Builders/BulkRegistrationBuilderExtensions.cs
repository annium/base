using System;
using Annium.Reflection;
using IBuilderBase = Annium.Core.DependencyInjection.IBulkRegistrationBuilderBase;
using IBuilderTarget = Annium.Core.DependencyInjection.IBulkRegistrationBuilderTarget;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for bulk registration builder configuration.
/// </summary>
public static class BulkRegistrationBuilderExtensions
{
    /// <summary>
    /// Filters types assignable to the specified generic type parameter.
    /// </summary>
    /// <typeparam name="T">The base type to filter by.</typeparam>
    /// <param name="builder">The registration builder.</param>
    /// <returns>The filtered registration builder.</returns>
    public static IBuilderBase AssignableTo<T>(this IBuilderBase builder) =>
        builder.Where(x => x.IsDerivedFrom(typeof(T)));

    /// <summary>
    /// Filters types assignable to the specified base type.
    /// </summary>
    /// <param name="builder">The registration builder.</param>
    /// <param name="baseType">The base type to filter by.</param>
    /// <returns>The filtered registration builder.</returns>
    public static IBuilderBase AssignableTo(this IBuilderBase builder, Type baseType) =>
        builder.Where(x => x.IsDerivedFrom(baseType));

    /// <summary>
    /// Filters types whose names start with the specified prefix.
    /// </summary>
    /// <param name="builder">The registration builder.</param>
    /// <param name="prefix">The prefix to match.</param>
    /// <returns>The filtered registration builder.</returns>
    public static IBuilderBase StartingWith(this IBuilderBase builder, string prefix) =>
        builder.Where(x => x.Name.StartsWith(prefix));

    /// <summary>
    /// Filters types whose names end with the specified suffix.
    /// </summary>
    /// <param name="builder">The registration builder.</param>
    /// <param name="suffix">The suffix to match.</param>
    /// <returns>The filtered registration builder.</returns>
    public static IBuilderBase EndingWith(this IBuilderBase builder, string suffix) =>
        builder.Where(x => x.Name.EndsWith(suffix));

    /// <summary>
    /// Registers the specified generic type as a service.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The registration builder.</param>
    /// <returns>The registration builder target.</returns>
    public static IBuilderTarget As<T>(this IBuilderTarget builder) => builder.As(typeof(T));

    /// <summary>
    /// Registers the specified generic type as a keyed service.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The registration builder.</param>
    /// <param name="getKey">A function to get the key for the service type.</param>
    /// <returns>The registration builder target.</returns>
    public static IBuilderTarget AsKeyed<T>(this IBuilderTarget builder, Func<Type, object> getKey) =>
        builder.AsKeyed(typeof(T), getKey);

    /// <summary>
    /// Registers the specified generic type as a factory service.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The registration builder.</param>
    /// <returns>The registration builder target.</returns>
    public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) => builder.AsFactory(typeof(T));

    /// <summary>
    /// Registers the specified generic type as a keyed factory service.
    /// </summary>
    /// <typeparam name="T">The service type to register.</typeparam>
    /// <param name="builder">The registration builder.</param>
    /// <param name="getKey">A function to get the key for the service type.</param>
    /// <returns>The registration builder target.</returns>
    public static IBuilderTarget AsKeyedFactory<T>(this IBuilderTarget builder, Func<Type, object> getKey) =>
        builder.AsKeyedFactory(typeof(T), getKey);
}
