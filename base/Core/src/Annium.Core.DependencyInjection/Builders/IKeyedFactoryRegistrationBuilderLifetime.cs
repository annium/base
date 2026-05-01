// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Interface for keyed factory registration builder lifetime operations.
/// </summary>
public interface IKeyedFactoryRegistrationBuilderLifetime
{
    /// <summary>
    /// Sets the service lifetime for the registration.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use</param>
    /// <returns>The service container instance</returns>
    IServiceContainer In(ServiceLifetime lifetime);

    /// <summary>
    /// Sets the service lifetime to scoped for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Scoped();

    /// <summary>
    /// Sets the service lifetime to singleton for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Singleton();

    /// <summary>
    /// Sets the service lifetime to transient for the registration.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Transient();
}
