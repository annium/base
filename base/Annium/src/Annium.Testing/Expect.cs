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
    /// Asynchronously waits until the specified synchronous validation action succeeds or the cancellation token is triggered.
    /// </summary>
    /// <param name="validate">The validation action to execute.</param>
    /// <param name="ct">The cancellation token to observe.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ToAsync(Action validate, CancellationToken ct, int pollDelay = 25)
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
    public static async Task ToAsync(Func<ValueTask> validate, CancellationToken ct, int pollDelay = 25)
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
    public static Task ToAsync(Action validate, int ms = 10_000, int pollDelay = 25) =>
        ToAsync(validate, new CancellationTokenSource(ms).Token, pollDelay);

    /// <summary>
    /// Asynchronously waits until the specified asynchronous validation function succeeds or the timeout is reached.
    /// </summary>
    /// <param name="validate">The asynchronous validation function to execute.</param>
    /// <param name="ms">The timeout in milliseconds.</param>
    /// <param name="pollDelay">The delay in milliseconds between validation attempts.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static Task ToAsync(Func<ValueTask> validate, int ms = 10_000, int pollDelay = 25) =>
        ToAsync(validate, new CancellationTokenSource(ms).Token, pollDelay);
}
