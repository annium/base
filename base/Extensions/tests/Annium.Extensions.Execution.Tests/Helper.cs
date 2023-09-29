using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Tests;

internal static class Helper
{
    public static Task AsyncLongWork() => AsyncWork(200);
    public static Task AsyncFastWork() => AsyncWork(20);

    public static void SyncLongWork() => SyncWork(400);
    public static void SyncFastWork() => SyncWork(40);

    private static async Task AsyncWork(int iterations)
    {
        var i = 0;
        while (i < iterations)
        {
            await Task.Delay(2);
            i++;
        }
    }

    private static void SyncWork(int delay)
    {
        SpinWait.SpinUntil(() => false, delay);
    }
}