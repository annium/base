using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime.Time;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of time configuration builder for setting up time providers
/// </summary>
internal class TimeConfigurationBuilder : ITimeConfigurationBuilder
{
    /// <summary>
    /// The selected time type
    /// </summary>
    private TimeType? _type;

    /// <summary>
    /// The service container to register services in
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The service lifetime for registered services
    /// </summary>
    private readonly ServiceLifetime _lifetime;

    /// <summary>
    /// Initializes a new instance of TimeConfigurationBuilder
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="lifetime">The service lifetime for registered services</param>
    public TimeConfigurationBuilder(IServiceContainer container, ServiceLifetime lifetime)
    {
        _container = container;
        _lifetime = lifetime;
    }

    /// <summary>
    /// Configures the time system to use real system time
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    public ITimeConfigurationBuilder WithRealTime()
    {
        _container.Add<RealTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Real).In(_lifetime);
        _container.Add<IActionScheduler, ActionScheduler>().In(_lifetime);
        _type = TimeType.Real;

        return this;
    }

    /// <summary>
    /// Configures the time system to use relative time starting from initialization
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    public ITimeConfigurationBuilder WithRelativeTime()
    {
        _container.Add<RelativeTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Relative).In(_lifetime);
        _container.Add<IActionScheduler, ActionScheduler>().In(_lifetime);
        _type = TimeType.Relative;

        return this;
    }

    /// <summary>
    /// Configures the time system to use managed time with manual control
    /// </summary>
    /// <returns>The configuration builder for method chaining</returns>
    public ITimeConfigurationBuilder WithManagedTime()
    {
        _container
            .Add<ITimeManager, ManagedTimeProvider>()
            .AsKeyed(typeof(IInternalTimeProvider), TimeType.Managed)
            .In(_lifetime);
        _container.Add<IActionScheduler, ManagedActionScheduler>().In(_lifetime);
        _type = TimeType.Managed;

        return this;
    }

    /// <summary>
    /// Sets the configured time provider as the default and completes configuration
    /// </summary>
    /// <returns>The configured service container</returns>
    public IServiceContainer SetDefault()
    {
        if (_type is null)
            throw new InvalidOperationException("Can't set default time provider - no providers registered");

        _container.Add(_type).AsSelf().Singleton();
        _container.Add<TimeProvider>().AsInterfaces().In(_lifetime);

        return _container;
    }
}
