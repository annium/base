// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Shared lifetime-terminator contract for every registration builder kind (Single, Bulk,
/// Factory, KeyedFactory). Each registration builder exposes these four methods as the final
/// step of the fluent chain.
/// </summary>
public interface IRegistrationBuilderLifetime
{
    /// <summary>
    /// Sets the service lifetime for the registration.
    /// </summary>
    /// <remarks>
    /// For type and bulk builders (<see cref="ISingleRegistrationBuilderBase"/>,
    /// <see cref="IBulkRegistrationBuilderBase"/>), committing without a prior
    /// <c>AsSelf</c> call implicitly registers each implementation type as itself. This
    /// self-registration is required so that factory-style descriptors produced by
    /// <c>As</c>, <c>AsInterfaces</c>, <c>AsKeyed*</c>, and <c>AsKeyedInterfaces</c> can
    /// resolve the implementation from the container.
    /// </remarks>
    /// <param name="lifetime">The service lifetime to use.</param>
    /// <returns>The service container instance.</returns>
    IServiceContainer In(ServiceLifetime lifetime);

    /// <summary>
    /// Sets the service lifetime to scoped for the registration.
    /// </summary>
    /// <returns>The service container instance.</returns>
    IServiceContainer Scoped();

    /// <summary>
    /// Sets the service lifetime to singleton for the registration.
    /// </summary>
    /// <returns>The service container instance.</returns>
    IServiceContainer Singleton();

    /// <summary>
    /// Sets the service lifetime to transient for the registration.
    /// </summary>
    /// <returns>The service container instance.</returns>
    IServiceContainer Transient();
}
