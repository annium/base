using System.Threading;
using NodaTime;

namespace Annium.Threading;

public static class CancellationTokenSourceExtensions
{
    public static void CancelAfter(this CancellationTokenSource cts, IActionScheduler scheduler, Duration duration)
    {
        scheduler.Delay(cts.Cancel, duration);
    }
}