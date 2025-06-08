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
}
