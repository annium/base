using System;

namespace Annium;

/// <summary>
/// Provides extension methods for service providers.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Injects a value into the service provider's container.
    /// </summary>
    /// <typeparam name="T">The type of the value to inject.</typeparam>
    /// <param name="sp">The service provider.</param>
    /// <param name="value">The value to inject.</param>
    /// <exception cref="InvalidOperationException">Thrown when the injection container is not found or already initialized.</exception>
    public static void Inject<T>(this IServiceProvider sp, T value)
        where T : class
    {
        sp.GetService(typeof(Injected<T>)).NotNull().CastTo<Injected<T>>().Init(value);
    }
}
