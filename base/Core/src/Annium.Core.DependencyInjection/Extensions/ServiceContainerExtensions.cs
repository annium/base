using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Core.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for service containers
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds a service with its implementation to the container
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="container">Service container</param>
    /// <returns>Registration builder</returns>
    public static ISingleRegistrationBuilderBase Add<TService, TImplementation>(this IServiceContainer container)
        where TImplementation : TService
    {
        return container.Add(typeof(TImplementation)).As<TService>();
    }

    /// <summary>
    /// Adds an implementation type to the container
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="container">Service container</param>
    /// <returns>Registration builder</returns>
    public static ISingleRegistrationBuilderBase Add<TImplementation>(this IServiceContainer container)
    {
        return container.Add(typeof(TImplementation));
    }

    /// <summary>
    /// Adds injectable services to the container
    /// </summary>
    /// <param name="container">Service container</param>
    /// <returns>Service container</returns>
    public static IServiceContainer AddInjectables(this IServiceContainer container)
    {
        return container.Add(typeof(Injected<>)).AsSelf().Scoped();
    }
}
