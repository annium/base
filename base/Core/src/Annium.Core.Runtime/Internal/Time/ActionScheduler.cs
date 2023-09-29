using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class ActionScheduler : IActionScheduler
{
    public Action Delay(Action handle, int timeout)
        => Delay(handle, Duration.FromMilliseconds(timeout));

    public Action Delay(Action handle, Duration timeout)
    {
        var execute = true;
        Task.Delay(timeout.ToTimeSpan()).ContinueWith(_ =>
        {
            if (execute)
                handle();
        });

        return () => execute = false;
    }

    public Action Interval(Action handle, int interval)
        => Interval(handle, Duration.FromMilliseconds(interval));

    public Action Interval(Action handle, Duration interval)
    {
        var span = interval.ToTimeSpan();
        var timer = new Timer(_ => handle(), null, span, span);

        return () => timer.Dispose();
    }
}