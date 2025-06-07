using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Threading.Tasks;

/// <summary>
/// Provides methods for awaiting conditions asynchronously.
/// </summary>
public static class Wait
{
    /// <summary>
    /// Awaits while the condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static async Task WhileAsync(Func<bool> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    /// <summary>
    /// Awaits while the condition is true or the task is canceled, with a timeout and poll delay.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<bool> condition, int ms, int pollDelay) =>
        WhileAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);

    /// <summary>
    /// Awaits while the condition is true or the task is canceled, with a timeout.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<bool> condition, int ms) =>
        WhileAsync(condition, new CancellationTokenSource(ms).Token);

    /// <summary>
    /// Awaits while the condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<bool> condition) => WhileAsync(condition, CancellationToken.None);

    /// <summary>
    /// Awaits while the asynchronous condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static async Task WhileAsync(Func<Task<bool>> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (await condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    /// <summary>
    /// Awaits while the asynchronous condition is true or the task is canceled, with a timeout and poll delay.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<Task<bool>> condition, int ms, int pollDelay) =>
        WhileAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);

    /// <summary>
    /// Awaits while the asynchronous condition is true or the task is canceled, with a timeout.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<Task<bool>> condition, int ms) =>
        WhileAsync(condition, new CancellationTokenSource(ms).Token);

    /// <summary>
    /// Awaits while the asynchronous condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <returns>A task that completes when the condition is false or canceled.</returns>
    public static Task WhileAsync(Func<Task<bool>> condition) => WhileAsync(condition, CancellationToken.None);

    /// <summary>
    /// Awaits until the condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static async Task UntilAsync(Func<bool> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (!condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    /// <summary>
    /// Awaits until the condition is true or the task is canceled, with a timeout and poll delay.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<bool> condition, int ms, int pollDelay) =>
        UntilAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);

    /// <summary>
    /// Awaits until the condition is true or the task is canceled, with a timeout.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<bool> condition, int ms) =>
        UntilAsync(condition, new CancellationTokenSource(ms).Token);

    /// <summary>
    /// Awaits until the condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<bool> condition) => UntilAsync(condition, CancellationToken.None);

    /// <summary>
    /// Awaits until the asynchronous condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static async Task UntilAsync(Func<Task<bool>> condition, CancellationToken ct, int pollDelay = 25)
    {
        while (!await condition() && !ct.IsCancellationRequested)
            await Task.Delay(pollDelay, CancellationToken.None).ConfigureAwait(true);
    }

    /// <summary>
    /// Awaits until the asynchronous condition is true or the task is canceled, with a timeout and poll delay.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay at which the condition will be polled, in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<Task<bool>> condition, int ms, int pollDelay) =>
        UntilAsync(condition, new CancellationTokenSource(ms).Token, pollDelay);

    /// <summary>
    /// Awaits until the asynchronous condition is true or the task is canceled, with a timeout.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <param name="ms">Timeout in milliseconds.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<Task<bool>> condition, int ms) =>
        UntilAsync(condition, new CancellationTokenSource(ms).Token);

    /// <summary>
    /// Awaits until the asynchronous condition is true or the task is canceled.
    /// </summary>
    /// <param name="condition">The asynchronous condition that will perpetuate the block.</param>
    /// <returns>A task that completes when the condition is true or canceled.</returns>
    public static Task UntilAsync(Func<Task<bool>> condition) => UntilAsync(condition, CancellationToken.None);
}
