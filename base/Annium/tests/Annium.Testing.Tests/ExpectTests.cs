using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="Expect"/> covering the <c>Func&lt;ValueTask&gt;</c> overloads of <c>ToAsync</c>.
/// </summary>
public class ExpectTests
{
    /// <summary>
    /// Verifies that <c>Expect.ToAsync(Func&lt;ValueTask&gt;, int)</c> polls until the condition succeeds
    /// and completes without throwing when the condition becomes true before the timeout.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_AsyncValidate_WaitsUntilConditionMet()
    {
        var ready = false;

        // Flip the flag after a short delay on a background task.
        _ = Task.Run(
            async () =>
            {
                await Task.Delay(50);
                ready = true;
            },
            TestContext.Current.CancellationToken
        );

        // Should complete without throwing once `ready` becomes true.
        await Expect.ToAsync(
            async () =>
            {
                await Task.Yield();
                if (!ready)
                    throw new InvalidOperationException("not yet ready");
            },
            ms: 3_000
        );

        ready.IsTrue();
    }

    /// <summary>
    /// Verifies that <c>Expect.ToAsync(Func&lt;ValueTask&gt;, int)</c> re-throws the validate lambda's
    /// exception when the condition never becomes true within the timeout window.
    /// The implementation swallows <see cref="OperationCanceledException"/> inside the poll loop and
    /// then calls <c>validate()</c> one final time — so the exception from the lambda propagates.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_AsyncValidate_TimesOut_ThrowsValidateLambdaException()
    {
        await Wrap.It(async () =>
            await Expect.ToAsync(
                async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("never ready");
                },
                ms: 50
            )
        ).ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that <c>Expect.ToAsync(Func&lt;ValueTask&gt;, CancellationToken)</c> honours an already-
    /// cancelled token: the poll loop exits immediately and the final validate call re-throws.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_AsyncValidate_PreCancelledToken_ThrowsValidateLambdaException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () =>
            await Expect.ToAsync(
                async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("never ready");
                },
                cts.Token
            )
        ).ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Sync-Action overload polls until the condition succeeds and completes once it does.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_SyncValidate_WaitsUntilConditionMet()
    {
        var ready = false;

        _ = Task.Run(
            async () =>
            {
                await Task.Delay(50);
                ready = true;
            },
            TestContext.Current.CancellationToken
        );

        await Expect.ToAsync(
            () =>
            {
                if (!ready)
                    throw new InvalidOperationException("not yet ready");
            },
            ms: 3_000
        );

        ready.IsTrue();
    }

    /// <summary>
    /// Sync-Action overload re-throws the validate lambda's exception when the timeout expires
    /// before the condition is met.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_SyncValidate_TimesOut_ThrowsValidateLambdaException()
    {
        await Wrap.It(async () =>
            await Expect.ToAsync(
                () => throw new InvalidOperationException("never ready"),
                ms: 50
            )
        ).ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// Sync-Action overload honours a pre-cancelled token: the poll loop exits immediately and
    /// the final validate call re-throws.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ToAsync_SyncValidate_PreCancelledToken_ThrowsValidateLambdaException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () =>
            await Expect.ToAsync(
                () => throw new InvalidOperationException("never ready"),
                cts.Token
            )
        ).ThrowsAsync<InvalidOperationException>();
    }
}
