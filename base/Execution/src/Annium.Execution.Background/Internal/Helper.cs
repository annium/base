using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Execution.Background.Internal;

/// <summary>
/// Helper class for running tasks in different execution contexts
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Runs a task in the background using Task.Run
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the background execution</returns>
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

    /// <summary>
    /// Runs a task in the foreground synchronously
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the execution</returns>
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
