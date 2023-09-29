using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;

namespace Annium.Core.Runtime.Internal.Time;

internal class TimeConfigurationBuilder : ITimeConfigurationBuilder
{
    private TimeType? _type;
    private readonly IServiceContainer _container;
    private readonly ServiceLifetime _lifetime;

    public TimeConfigurationBuilder(
        IServiceContainer container,
        ServiceLifetime lifetime
    )
    {
        _container = container;
        _lifetime = lifetime;
    }

    public ITimeConfigurationBuilder WithRealTime()
    {
        _container.Add<RealTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Real).In(_lifetime);
        _container.Add<IActionScheduler, ActionScheduler>().In(_lifetime);
        _type = TimeType.Real;

        return this;
    }

    public ITimeConfigurationBuilder WithRelativeTime()
    {
        _container.Add<RelativeTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Relative).In(_lifetime);
        _container.Add<IActionScheduler, ActionScheduler>().In(_lifetime);
        _type = TimeType.Relative;

        return this;
    }

    public ITimeConfigurationBuilder WithManagedTime()
    {
        _container.Add<ITimeManager, ManagedTimeProvider>().AsKeyed(typeof(IInternalTimeProvider), TimeType.Managed).In(_lifetime);
        _container.Add<IActionScheduler, ManagedActionScheduler>().In(_lifetime);
        _type = TimeType.Managed;

        return this;
    }

    public IServiceContainer SetDefault()
    {
        if (_type is null)
            throw new InvalidOperationException("Can't set default time provider - no providers registered");

        _container.Add(_type).AsSelf().Singleton();
        _container.Add<TimeProvider>().AsInterfaces().In(_lifetime);

        return _container;
    }
}