using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of time provider that can switch between different time providers
/// </summary>
internal class TimeProvider : ITimeProviderSwitcher, ITimeProvider
{
    /// <summary>
    /// The current instant in time from the active provider
    /// </summary>
    public Instant Now => _provider.Now;

    /// <summary>
    /// The current date and time as UTC DateTime from the active provider
    /// </summary>
    public DateTime DateTimeNow => _provider.DateTimeNow;

    /// <summary>
    /// The current time as Unix timestamp in milliseconds from the active provider
    /// </summary>
    public long UnixMsNow => _provider.UnixMsNow;

    /// <summary>
    /// The current time as Unix timestamp in seconds from the active provider
    /// </summary>
    public long UnixSecondsNow => _provider.UnixSecondsNow;

    /// <summary>
    /// The currently active time provider
    /// </summary>
    private IInternalTimeProvider _provider;

    /// <summary>
    /// The service provider for resolving time providers
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Initializes a new instance of TimeProvider with the specified default type
    /// </summary>
    /// <param name="sp">The service provider for resolving time providers</param>
    /// <param name="defaultType">The default time provider type to use</param>
    public TimeProvider(IServiceProvider sp, TimeType defaultType)
    {
        _sp = sp;
        _provider = _sp.ResolveKeyed<IInternalTimeProvider>(defaultType);
    }

    /// <summary>
    /// Switches to using real system time
    /// </summary>
    public void UseRealTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Real);

    /// <summary>
    /// Switches to using relative time
    /// </summary>
    public void UseRelativeTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Relative);

    /// <summary>
    /// Switches to using managed time
    /// </summary>
    public void UseManagedTime() => _provider = _sp.ResolveKeyed<IInternalTimeProvider>(TimeType.Managed);
}
