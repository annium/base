using System;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class ManagedActionScheduler : IActionScheduler
{
    private readonly ITimeManager _timeManager;

    public ManagedActionScheduler(
        ITimeManager timeManager
    )
    {
        _timeManager = timeManager;
    }

    public Action Delay(Action handle, int timeout)
        => Delay(handle, Duration.FromMilliseconds(timeout));

    public Action Delay(Action handle, Duration timeout)
    {
        var lasting = Duration.Zero;

        _timeManager.NowChanged += CheckTime;

        void CheckTime(Duration duration)
        {
            lasting += duration;
            if (lasting < timeout)
                return;
            _timeManager.NowChanged -= CheckTime;
            handle();
        }

        return () => _timeManager.NowChanged -= CheckTime;
    }

    public Action Interval(Action handle, int interval)
        => Interval(handle, Duration.FromMilliseconds(interval));

    public Action Interval(Action handle, Duration interval)
    {
        var lasting = Duration.Zero;

        _timeManager.NowChanged += CheckTime;

        void CheckTime(Duration duration)
        {
            lasting += duration;
            if (lasting < interval)
                return;

            lasting -= interval;
            handle();
        }

        return () => _timeManager.NowChanged -= CheckTime;
    }
}