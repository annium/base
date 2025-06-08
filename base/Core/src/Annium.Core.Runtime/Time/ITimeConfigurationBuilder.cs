using Annium.Core.DependencyInjection;

namespace Annium.Core.Runtime.Time;

/// <summary>
/// Builder interface for configuring time providers in the application
/// </summary>
public interface ITimeConfigurationBuilder
{
    /// <summary>
    /// Configures the time system to use real system time
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    ITimeConfigurationBuilder WithRealTime();

    /// <summary>
    /// Configures the time system to use relative time starting from initialization
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    ITimeConfigurationBuilder WithRelativeTime();

    /// <summary>
    /// Configures the time system to use managed time with manual control
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    ITimeConfigurationBuilder WithManagedTime();

    /// <summary>
    /// Sets the configured time provider as the default and completes configuration
    /// </summary>
    /// <returns>The configured service container</returns>
    IServiceContainer SetDefault();
}
