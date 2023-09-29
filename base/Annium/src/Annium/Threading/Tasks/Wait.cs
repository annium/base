using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

public static class Wait
{
    /// <summary>
    ///     Awaits while condition is true or task is canceled.
    /// </summary>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="ct">
    ///     Cancellation token.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task WhileAsync(Func<bool> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    public static Task WhileAsync(Func<bool> condition, int ms, int pollDelay) => WhileAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);
    public static Task WhileAsync(Func<bool> condition, int ms) => WhileAsync(condition, new CancellationTokenSource(ms).Token);
    public static Task WhileAsync(Func<bool> condition) => WhileAsync(condition, CancellationToken.None);

    /// <summary>
    ///     Awaits until condition is true or task is canceled.
    /// </summary>
    /// <param name="condition">
    ///     The condition that will perpetuate the block.
    /// </param>
    /// <param name="ct">
    ///     Cancellation token.
    /// </param>
    /// <param name="pollDelay">
    ///     The delay at which the condition will be polled, in milliseconds.
    /// </param>
    /// <returns>
    ///     <see cref="Task" />.
    /// </returns>
    public static async Task UntilAsync(Func<bool> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (!condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    public static Task UntilAsync(Func<bool> condition, int ms, int pollDelay) => UntilAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);
    public static Task UntilAsync(Func<bool> condition, int ms) => UntilAsync(condition, new CancellationTokenSource(ms).Token);
    public static Task UntilAsync(Func<bool> condition) => UntilAsync(condition, CancellationToken.None);
}