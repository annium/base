using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Execution.Background.Internal;

internal static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task RunTaskInBackgroundAsync(Delegate task, CancellationToken ct) =>
        task switch
        {
            Action execute => Task.Run(execute, CancellationToken.None),
            Action<CancellationToken> execute => Task.Run(() => execute(ct), CancellationToken.None),
            Func<ValueTask> execute => Task.Run(
                async () => await execute().ConfigureAwait(false),
                CancellationToken.None
            ),
            Func<CancellationToken, ValueTask> execute => Task.Run(
                async () => await execute(ct).ConfigureAwait(false),
                CancellationToken.None
            ),
            _ => throw new NotSupportedException(),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask RunTaskInForegroundAsync(Delegate task, CancellationToken ct)
    {
        switch (task)
        {
            case Action t:
                t();
                break;
            case Action<CancellationToken> t:
                t(ct);
                break;
            case Func<ValueTask> t:
                await t();
                break;
            case Func<CancellationToken, ValueTask> t:
                await t(ct);
                break;
            default:
                throw new NotSupportedException();
        }
    }
}
