using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal.Background;

internal static class Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task RunTaskInBackground(Delegate task, CancellationToken ct) => task switch
    {
        Action execute                             => Task.Run(execute, CancellationToken.None),
        Action<CancellationToken> execute          => Task.Run(() => execute(ct), CancellationToken.None),
        Func<ValueTask> execute                    => Task.Run(async () => await execute().ConfigureAwait(false), CancellationToken.None),
        Func<CancellationToken, ValueTask> execute => Task.Run(async () => await execute(ct).ConfigureAwait(false), CancellationToken.None),
        _                                          => throw new NotSupportedException()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask RunTaskInForeground(Delegate task, CancellationToken ct)
    {
        try
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
        catch
        {
            // ignored by design - exception must be handled by task itself
        }
    }
}