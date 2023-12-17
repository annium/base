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
    private readonly IServiceProvider _sp;

    public TimeProvider(IServiceProvider sp, TimeType defaultType)
    {
        _sp = sp;
        _provider = _sp.ResolveKeyed<IInternalTimeProvider>(defaultType);
    }

    public void UseRealTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Real);

    public void UseRelativeTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Relative);

    public void UseManagedTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Managed);
}
