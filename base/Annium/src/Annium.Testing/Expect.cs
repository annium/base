using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

namespace Annium.Testing;

/// <summary>
/// Provides assertion methods for asynchronous expectations in tests.
/// </summary>
public static class Expect
{
    /// <summary>
    /// Default poll cadence in milliseconds shared by every ToAsync overload below.
    /// </summary>
    private const int DefaultPollDelayMs = 25;

    /// <summary>
    /// Default timeout in milliseconds shared by the no-CancellationToken ToAsync overloads.
    /// </summary>
    private const int DefaultTimeoutMs = 10_000;

    /// <summary>
    /// Asynchronously waits until the specified synchronous validation action succeeds or the cancellation token is triggered.
    /// </summary>
    /// <param name="validate">The validation action to execute.</param>
    /// <param name="ct">The cancellation token to observe.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ToAsync(Action validate, CancellationToken ct, int pollDelay = DefaultPollDelayMs)
    {
        await Wait.UntilAsync(
            () =>
            {
                try
                {
                    validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            },
            ct,
            pollDelay
        );
        validate();
    }

    /// <summary>
    /// Asynchronously waits until the specified asynchronous validation function succeeds or the cancellation token is triggered.
    /// </summary>
    /// <param name="validate">The asynchronous validation function to execute.</param>
    /// <param name="ct">The cancellation token to observe.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ToAsync(Func<ValueTask> validate, CancellationToken ct, int pollDelay = DefaultPollDelayMs)
    {
        await Wait.UntilAsync(
            async () =>
            {
                try
                {
                    await validate();
                    return true;
                }
                catch
                {
                    return false;
                }
            },
            ct,
            pollDelay
        );
        await validate();
    }

    /// <summary>
    /// Asynchronously waits until the specified synchronous validation action succeeds or the timeout is reached.
    /// </summary>
    /// <param name="validate">The validation action to execute.</param>
    /// <param name="ms">The timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ToAsync(Action validate, int ms = DefaultTimeoutMs, int pollDelay = DefaultPollDelayMs)
    {
        using var cts = new CancellationTokenSource(ms);
        await ToAsync(validate, cts.Token, pollDelay);
    }

    /// <summary>
    /// Asynchronously waits until the specified asynchronous validation function succeeds or the timeout is reached.
    /// </summary>
    /// <param name="validate">The asynchronous validation function to execute.</param>
    /// <param name="ms">The timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ToAsync(
        Func<ValueTask> validate,
        int ms = DefaultTimeoutMs,
        int pollDelay = DefaultPollDelayMs
    )
    {
        using var cts = new CancellationTokenSource(ms);
        await ToAsync(validate, cts.Token, pollDelay);
    }
}
