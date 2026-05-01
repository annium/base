// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Interface for bulk registration builder lifetime operations.
/// </summary>
public interface IBulkRegistrationBuilderLifetime
{
    /// <summary>
    /// Sets the service lifetime for all registrations.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use</param>
    /// <returns>The service container instance</returns>
    IServiceContainer In(ServiceLifetime lifetime);

    /// <summary>
    /// Sets the service lifetime to scoped for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Scoped();

    /// <summary>
    /// Sets the service lifetime to singleton for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Singleton();

    /// <summary>
    /// Sets the service lifetime to transient for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Transient();
}
