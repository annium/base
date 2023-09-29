using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class TimeProvider : ITimeProviderSwitcher, ITimeProvider
{
    public Instant Now => _provider.Now;

    public DateTime DateTimeNow => _provider.DateTimeNow;

    public long UnixMsNow => _provider.UnixMsNow;

    public long UnixSecondsNow => _provider.UnixSecondsNow;

    private IInternalTimeProvider _provider;
    private readonly IIndex<TimeType, IInternalTimeProvider> _timeProviders;

    public TimeProvider(IIndex<TimeType, IInternalTimeProvider> timeProviders, TimeType defaultType)
    {
        _provider = timeProviders[defaultType];
        _timeProviders = timeProviders;
    }

    public void UseRealTime()
        => _provider = _timeProviders[TimeType.Real];

    public void UseRelativeTime()
        => _provider = _timeProviders[TimeType.Relative];

    public void UseManagedTime()
        => _provider = _timeProviders[TimeType.Managed];
}